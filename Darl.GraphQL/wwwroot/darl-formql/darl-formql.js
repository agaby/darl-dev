// A library to create a form/questionnaire from the darl.dev GraphQL endpoint
// relies on graphql.js here https://github.com/f/graphql.js
var currentQSP;
var darlUrl;
var rootDiv;
var isValid = null;
var graph;

async function DARLForm(div, id, url, debug) {
    darlUrl = url;
    graph = graphql(url, { headers: { "Authorization": "Basic 8952d1af-9d34-4866-a4bc-412bf51743d6" } });
    var firstQSP = graph(`query beginForm($ruleset: String!){ beginQuestionnaire(ruleSetName: $ruleset){ ieToken questionHeader questions { text categories reference questionType maxval minval format dResponse sResponse }}}`);
    var content = await firstQSP({ ruleset: id });
    var data = content.beginQuestionnaire;
    if ( data === null) {
        var root = $(div);
        root.append($('<p></p>').text("This ruleset can't be found or has timed out. Refresh the page."));
    }
    else {
        BuildForm(div, data, debug);
        currentQSP = data;
    }
}

//
function BuildForm(div, qsp, debug) {
    rootDiv = div;
    var root = $('<form></form>').attr({ 'id': 'darlencform' });

    var submitForm = function (e) {
        console.log('form submit');
        //prevent form from submitting valid or invalid
        e.preventDefault();
        //user clicked and the form was not valid
        if (isValid === false) {
            isValid = null;
            return false;
        }
        //user pressed enter, process as if they clicked next instead
        $('#nextButton').trigger('click');
    };

    root.submit(submitForm);

    $(div).append(root);
    var header = $("<h4></h4>").text(qsp.preamble);
    root.append(header);
    if (qsp.questions !== null) {
        root.append($("<h4></h4>").text(qsp.questionHeader));
        //iterate over questions
        var i;
        for (i = 0; i < qsp.questions.length; i++) {
            var q = qsp.questions[i];
            var outerdiv = $('<div></div>').attr('class', 'form-group row');
            outerdiv.append($('<div></div>').attr('class', 'control-label col-md-4').text(q.text));
            var innerdiv = $('<div></div>').attr('class', 'col-md-8');
            switch (q.qtype) {
                case 0:
                    var numinput = $('<input/>').attr({ class: "form-control", 'data-val': 'true', 'data-val-number': 'the response must be a number', 'data-val-required': 'The field is required', id: 'form' + q.reference, max: q.maxval, min: q.minval, name: 'form' + q.reference, step: q.increment, value: q.dResponse, 'type': 'number' });
                    var numval = $('<span></span').attr({ class: "field-validation-valid", 'data-valmsg-for': 'form' + q.reference, 'data-valmsg-replace': 'true' });
                    innerdiv.append(numinput, numval);
                    break;
                case 1:
                    var catinput = $('<select></select>').attr({ class: "form-control", name: 'form' + q.reference, id: 'form' + q.reference });
                    for (p = 0; p < q.categories.length; p++) {
                        catinput.append($('<option></option>').text(q.categories[p]));
                    }
                    var catval = $('<span></span').attr({ class: "field-validation-valid", 'data-valmsg-for': 'form' + q.reference, 'data-valmsg-replace': 'true' });
                    innerdiv.append(catinput, catval);
                    break;
                case 2:
                    if (q.format !== "") { //chrome doesn't like empty pattern
                        var textinput = $('<input/>').attr({ class: "form-control widetextbox", 'data-val': 'true', 'data-val-required': 'The field is required', id: 'form' + q.reference, name: 'form' + q.reference, value: q.sResponse, pattern: q.format, 'type': 'text' });
                    }
                    else {
                        var textinput = $('<input/>').attr({ class: "form-control widetextbox", 'data-val': 'true', 'data-val-required': 'The field is required', id: 'form' + q.reference, name: 'form' + q.reference, value: q.sResponse, 'type': 'text' });
                    }
                    var textval = $('<span></span').attr({ class: "field-validation-valid", 'data-valmsg-for': 'form' + q.reference, 'data-valmsg-replace': 'true' });
                    innerdiv.append(textinput, textval);
                    break;
            }
            outerdiv.append(innerdiv);
            root.append(outerdiv);
        }
    }
    if (qsp.responses !== null) {
        root.append($("<h4></h4>").text(qsp.responseHeader));
        //iterate over responses
        var n;
        for (n = 0; n < qsp.responses.length; n++) {
            var r = qsp.responses[n];
            var outerrespdiv = $('<div></div>').attr('class', 'form-group row');
            switch (r.rtype) {
                case 0:
                    outerrespdiv.append($('<div></div>').attr('class', 'well well-lg').text(r.preamble));
                    break;
                case 1:
                    outerrespdiv.append($('<div></div>').attr('class', 'control-label col-sm-4').text(r.annotation));
                    outerrespdiv.append($('<div></div>').attr('class', 'col-sm-6 multiline').text(r.mainText));
                    break;
                case 2:
                    outerrespdiv.append($('<div></div>').attr('class', 'control-label col-sm-6').text(r.annotation));
                    outerrespdiv.append($('<div></div>').attr('class', 'col-sm-1').text(r.lowText));
                    outerrespdiv.append($('<div></div>').attr({
                        class: 'progress-bar progress-bar-info', style: 'width:' + r.value + '%', 'background-image': 'none', 'background-color': r.color
                    }).text(r.value));
                    outerrespdiv.append($('<div></div>').attr('class', 'col-sm-1').text(r.highText));
                    break;
                case 3:
                    var innerrespdiv = $('<div></div>').attr('class', 'control-label col-sm-4')
                    innerrespdiv.append($('<a></a>').attr({ href: 'r.mainText', class: 'btn btn-primary', style: 'opacity:' + r.format }).text(r.annotation));
                    outerrespdiv.append(innerrespdiv);
                    break;
            }
            root.append(outerrespdiv);
        }
    }
    //buttons
    var buttonsFormDiv = $('<div></div>').attr('class', 'form-group row');
    var buttonsDiv = $('<div></div>').attr('class', 'col-sm-offset-4 col-sm-10');
    var back = $('<input/>').attr({ class: "btn btn-primary", value: 'Back', type: 'submit', name: "backButton", 'id': 'backButton' });
    var next = $('<input/>').attr({ class: "btn btn-primary", value: 'Next', type: 'submit', name: "nextButton", 'id': 'nextButton' });
    if (qsp.canUnwind !== true) {
        back.attr('disabled', 'disabled');
    }
    if (qsp.complete === true) {
        next.attr('disabled', 'disabled');
    }
    buttonsDiv.append(back, next);
    buttonsFormDiv.append(buttonsDiv);
    root.append(buttonsFormDiv);
    //progress
    if (qsp.questions !== null) {
        var progressDiv = $('<div></div>').attr('class', 'form-group row');
        progressDiv.append($('<div></div>').attr('class', 'control-label col-sm-4').text('Progress'));
        progressDiv.append($('<div></div>').attr('class', 'col-sm-offset-4 col-sm-6').append($('<div></div>').attr({ class: 'progress-bar progress-bar-info', style: 'width:' + qsp.percentComplete + '%' }).text(qsp.percentComplete + '%')));

        root.append(progressDiv);
    }
    if (debug === true) {
        //saliences/values
        var reportingDiv = $('<div></div>').attr('class', 'panel panel-default');
        var headerText;
        var tableHead;
        if (qsp.complete) {
            headerText = "Show full results";
            tableHead = "Value";
        }
        else {
            headerText = "Show saliences";
            tableHead = "Salience";
        }
        reportingDiv.append($('<div></div>').attr('class', 'panel-heading').append($('<h4></h4>').attr('class', 'panel-title').append($('<a></a>').attr({ href: '#darlDebugValues', 'data-toggle': 'collapse' }).text(headerText))));
        var variablesTable = $('<table></table>').attr('class', 'table').append($('<thead></thead').append($('<tr></tr>').append($('<th></th>').text('Variable'), $('<th></th>').text(tableHead))));
        var variablesBody = $('<tbody></tbody>');
        for (var key in qsp.values) {
            variablesBody.append($('<tr></tr>').append($('<td></td>').text(key), $('<td></td>').text(qsp.values[key])));
        }

        variablesTable.append(variablesBody);
        reportingDiv.append($('<div></div>').attr({ 'class': 'panel-collapse collapse', id: 'darlDebugValues' }).append($('<div></div>').attr('class', 'panel-body').append(variablesTable)));
        root.append(reportingDiv);
    }

    $('#nextButton').on('click', GoNext);
    $('#backButton').on('click', GoBack);

}

function GoBack(e) {
    currentQSP.questionsRequested = -1;
    $.ajax({
        type: 'POST',
        url: darlUrl,
        data: JSON.stringify(currentQSP),
        success: function (data) {
            $(rootDiv).empty();
            BuildForm(rootDiv, data);
            currentQSP = data;
        },
        contentType: "application/json",
        dataType: 'json'
    });
    e.preventDefault(); //no submit
}

function GoNext(e) {
    //collect data
    var valid = true;
    if (currentQSP.questions !== null) {
        var i;
        for (i = 0; i < currentQSP.questions.length; i++) {
            var q = currentQSP.questions[i];
            if (!$('#form' + q.reference)[0].checkValidity())
                valid = false;
            switch (q.qtype) {
                case 0:
                    q.dResponse = $('#form' + q.reference).val();
                    break;
                case 1:
                case 2:
                    q.sResponse = $('#form' + q.reference).val();
                    break;
            }
        }
    }
    if (valid === true) {
        isValid = true;
        e.preventDefault(); //no submit
        $.ajax({
            type: 'POST',
            url: darlUrl,
            data: JSON.stringify(currentQSP),
            success: function (data) {
                $(rootDiv).empty();
                BuildForm(rootDiv, data);
                currentQSP = data;
            },
            contentType: "application/json",
            dataType: 'json'
        });
    }
    else {
        isValid = false;
        return true; //default behaviour if invalid
    }


}

