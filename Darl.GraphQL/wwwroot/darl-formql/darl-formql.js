// A library to create a form/questionnaire from the darl.dev GraphQL endpoint
// relies on graphql.js here https://github.com/f/graphql.js
var currentQSP;
var rootDiv;
var isValid = null;
var graph;
var nextQSP;
var backQSP;
var isDebug;
var url = "https://localhost:44311/graphql"; //"https://darl.dev/graphql"

async function DARLForm(div, id, debug, apiKey) {
    try {
        if (apiKey === null) {
            graph = graphql(url);
        }
        else {
            graph = graphql(url, { headers: { "Authorization": "Basic " + apiKey } });
        }
        isDebug = debug;
        var firstQSP = graph(`query beginForm($ruleset: String!){ beginQuestionnaire(ruleSetName: $ruleset){ ieToken questionHeader percentComplete canUnwind questions { text categories reference qType maxval minval format dResponse sResponse } values {name value}}}`);
        nextQSP = graph('query nextStep($ieToken: String!, $reference: String!, $qType: QuestionType!, $sresponse: String, $dresponse: Float   ){ continueQuestionnaire(responses: {ieToken: $ieToken, questions:[{reference: $reference, sResponse: $sresponse,dResponse: $dresponse, qType: $qType}]}) { complete ieToken questionHeader percentComplete canUnwind values {name value } questions { text categories  reference qType sResponse  dResponse} responses { mainText annotation rType preamble}}}');
        backQSP = graph('query back($ieToken: String!){ backtrackQuestionnaire(ieToken: $ieToken) {complete ieToken questionHeader percentComplete canUnwind values { name value } questions { text categories  reference qType sResponse  dResponse } responses { mainText annotation rType preamble }}}');
        var content = await firstQSP({ ruleset: id });

        var data = content.beginQuestionnaire;
        if (data === null) {
            var root = $(div);
            root.append($('<p></p>').text("This ruleset can't be found or has timed out. Refresh the page."));
        }
        else {
            $(div).empty();
            BuildForm(div, data, debug);
            currentQSP = data;
        }
    }
    catch (err) {
        alert(err[0].message);
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
      //  $('#nextButton').trigger('click');
    };

    root.submit(submitForm);

    $(div).append(root);
    var header = $("<h4></h4>").text(qsp.preamble);
    root.append(header);
    if (qsp.questions !== null && qsp.questions !== undefined) {
        root.append($("<h4></h4>").text(qsp.questionHeader));
        //iterate over questions
        var i;
        for (i = 0; i < qsp.questions.length; i++) {
            var q = qsp.questions[i];
            var outerdiv = $('<div></div>').attr('class', 'form-group row');
            outerdiv.append($('<div></div>').attr('class', 'control-label col-md-4').text(q.text));
            var innerdiv = $('<div></div>').attr('class', 'col-md-8');
            switch (q.qType) {
                case 'numeric':
                    var numinput = $('<input/>').attr({ class: "form-control", 'data-val': 'true', 'data-val-number': 'the response must be a number', 'data-val-required': 'The field is required', id: 'form' + q.reference, max: q.maxval, min: q.minval, name: 'form' + q.reference, step: q.increment, value: q.dResponse, 'type': 'number' });
                    var numval = $('<span></span').attr({ class: "field-validation-valid", 'data-valmsg-for': 'form' + q.reference, 'data-valmsg-replace': 'true' });
                    innerdiv.append(numinput, numval);
                    break;
                case 'categorical':
                    var catinput = $('<select></select>').attr({ class: "form-control", name: 'form' + q.reference, id: 'form' + q.reference });
                    for (p = 0; p < q.categories.length; p++) {
                        catinput.append($('<option></option>').text(q.categories[p]));
                    }
                    var catval = $('<span></span').attr({ class: "field-validation-valid", 'data-valmsg-for': 'form' + q.reference, 'data-valmsg-replace': 'true' });
                    innerdiv.append(catinput, catval);
                    break;
                case 'textual':
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
    if (qsp.responses !== null && qsp.responses !== undefined) {
        root.append($("<h4></h4>").text(qsp.responseHeader));
        //iterate over responses
        var n;
        for (n = 0; n < qsp.responses.length; n++) {
            var r = qsp.responses[n];
            var outerrespdiv = $('<div></div>').attr('class', 'form-group row');
            switch (r.rType) {
                case 'Preamble':
                    outerrespdiv.append($('<div></div>').attr('class', 'well well-lg').text(r.preamble));
                    break;
                case 'Text':
                    outerrespdiv.append($('<div></div>').attr('class', 'control-label col-sm-4').text(r.annotation));
                    outerrespdiv.append($('<div></div>').attr('class', 'col-sm-6 multiline').text(r.mainText));
                    break;
                case 'ScoreBar':
                    outerrespdiv.append($('<div></div>').attr('class', 'control-label col-sm-6').text(r.annotation));
                    outerrespdiv.append($('<div></div>').attr('class', 'col-sm-1').text(r.lowText));
                    outerrespdiv.append($('<div></div>').attr({
                        class: 'progress-bar progress-bar-info', style: 'width:' + r.value + '%', 'background-image': 'none', 'background-color': r.color
                    }).text(r.value));
                    outerrespdiv.append($('<div></div>').attr('class', 'col-sm-1').text(r.highText));
                    break;
                case 'Link':
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
            variablesBody.append($('<tr></tr>').append($('<td></td>').text(qsp.values[key].name), $('<td></td>').text(qsp.values[key].value)));
        }

        variablesTable.append(variablesBody);
        reportingDiv.append($('<div></div>').attr({ 'class': 'panel-collapse collapse', id: 'darlDebugValues' }).append($('<div></div>').attr('class', 'panel-body').append(variablesTable)));
        root.append(reportingDiv);
    }
    $('#nextButton').on('click', GoNext);
    $('#backButton').on('click', GoBack);

}

async function GoBack(e) {
    try {
        var bqsp = await backQSP({ ieToken: currentQSP.ieToken });
        var data = bqsp.backtrackQuestionnaire;
        $(rootDiv).empty();
        BuildForm(rootDiv, data, isDebug);
        currentQSP = data;
        e.preventDefault(); //no submit
    }
    catch (err) {
        alert(err[0].message);
    }
}

async function GoNext(e) {
    //collect data
    var valid = true;
    if (currentQSP.questions !== null) {
        var i;
        for (i = 0; i < currentQSP.questions.length; i++) {
            var q = currentQSP.questions[i];
            if (!$('#form' + q.reference)[0].checkValidity())
                valid = false;
            switch (q.qType) {
                case 'numeric':
                    q.dResponse = $('#form' + q.reference).val();
                    break;
                case 'categorical':
                case 'textual':
                    q.sResponse = $('#form' + q.reference).val();
                    break;
            }
        }
    }
    if (valid === true) {
        try {
            isValid = true;
            e.preventDefault(); //no submit
            var nqsp = await nextQSP({ ieToken: currentQSP.ieToken, reference: currentQSP.questions[0].reference, qType: currentQSP.questions[0].qType, sresponse: currentQSP.questions[0].sResponse, dresponse: currentQSP.questions[0].dResponse });
            $(rootDiv).empty();
            var data = nqsp.continueQuestionnaire;
            BuildForm(rootDiv, data,isDebug);
            currentQSP = data;
        }
        catch (err) {
            alert(err[0].message);
        }
    }
    else {
        isValid = false;
        return true; //default behavior if invalid
    }


}

