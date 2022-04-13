var graph;
var demo;
var nodasource;
var interact;
var kgname;
var currentStateId;


$(async function () {
    currentStateId = uuidv4();
    var url = window.location.origin;
    var key = "";
    if (url === "https://localhost:44311/index")
        url = "https://localhost:44311";
    graph = graphql(url + "/graphql");
    var apiKey = findGetParameter("apikey");
    if ($('#kgurl').data('kgurl')) {
        mdname = $('#kgurl').data('kgurl');
    }
    else {
        mdname = findGetParameter("kgraph");
    }

    if (apiKey !== null)
        graph = graphql(url + "/graphql", { headers: { "Authorization": "Basic " + apiKey } });
    else if (key !== null && key !== "")
        graph = graphql(url + "/graphql", { headers: { "Authorization": "Basic " + key } });
    else if (!$('#auth').length) {
        demo = true;
    }
    interact = graph('query int($name: String! $ksid: String! $text:  String!){interactKnowledgeGraph(kgModelName: $name conversationId: $ksid conversationData: { dataType: textual name: "" value: $text }){ darl reference activeNodes response{dataType name value categories{name value }}}}');
    nodaSource = graph('query ($name: String!){exportNoda(graphName: $name)}');

    $('#kgmodel-dropdown').on('change', async function () {
        kgname = this.value;
    });

    $('#kg-build').click(async function () {
        try {
            if (kgname.length) {
                await Build();
            }
            else {
                alert("Please select a knowledge graph.");
            }
        }
        catch (err) {
            HandleError(err);
        }
    });

    $('.msg_send_btn').click(async function () {
        const text = $('.write_msg').val();
        if (text !== "")
            await HandleChatText(text);
    });
});

async function Build() {
    var res = await nodaSource({ name: kgname });
    var root = JSON.parse(res.exportNoda);
    $('#msg_input').val(root.initialText)
    var converter = new showdown.Converter();
    var html = converter.makeHtml(root.description);
    $('#kg-description').html(html);
    //now create the network in noda

}

function findGetParameter(parameterName) {
    var result = null,
        tmp = [];
    location.search
        .substr(1)
        .split("&")
        .forEach(function (item) {
            tmp = item.split("=");
            if (tmp[0] === parameterName) result = decodeURIComponent(tmp[1]);
        });
    return result;
}

async function HandleChatText(text) {
    try {
        $('.write_msg').val('');
        AddOutGoingText(text);
        var res = await interact({ name: kgname, ksid: currentStateId, text: text });
        AddInComingMessage(res);
        $(".msg_history").stop().animate({ scrollTop: $(".msg_history")[0].scrollHeight }, 1000);
        //highlight appropriate nodes here.
    }
    catch (err) {
        HandleError(err);
    }
}

function AddIncomingText(text) {
    var converter = new showdown.Converter();
    var html = converter.makeHtml(text);
    $('.msg_history').append('<div class="incoming_msg">' +
        '<div class="received_msg">' +
        '<div class="received_withd_msg">' +
        '<p>' + html + '</p>' +
        '</div>' +
        '</div>' +
        '</div>');
}

function AddOutGoingText(text) {
    $('.msg_history').append('<div class="outgoing_msg">' +
        '<div class="sent_msg">' +
        '<p>' + text + '</p>' +
        '</div>' +
        '</div>');
}

function HandleError(err) {
    if (Array.isArray(err)) {
        alert(err[0].message);
    }
    else {
        alert(err);
    }
}

function AddInComingMessage(message) {
    //remove any previous buttons
    $('.received_withd_msg > .btn-group').empty();
    for (let i = 0, n = message.interactKnowledgeGraph.length; i < n; i++) {
        let r = message.interactKnowledgeGraph[i];
        switch (r.response.dataType) {
            case "textual":
            case "numeric":
                AddIncomingText(r.response.value);
                break;
            case "categorical":
                AddIncomingText(r.response.value);
                var cats = "";
                for (let n of r.response.categories) {
                    cats = cats + '<button type="button" class="btn btn-secondary chat-btn">' + n.name + '</button>';
                }
                $('.msg_history').append('<div class="incoming_msg">' +
                    '<div class="received_msg">' +
                    '<div class="received_withd_msg">' +
                    '<div class="btn-group" role="group">' + cats + '</div>' +
                    '</div>' +
                    '</div>' +
                    '</div>');
                $('.chat-btn').click(async function (data) {
                    const text = $(data.target).text();
                    await HandleChatText(text);
                });
                break;
        }
    }
}

function CreateNodaNode(kgnode) {
    var nodeProps = {};

    nodeProps.uuid = document.getElementById('nodeUuid').value;
    nodeProps.title = document.getElementById('nodeTitle').value;
    nodeProps.color = document.getElementById('nodeColor').value;
    nodeProps.opacity = parseFloat(document.getElementById('nodeOpacity').value);
    nodeProps.shape = document.getElementById('nodeShape').value;
    nodeProps.imageUrl = document.getElementById('nodeImageUrl').value;
    nodeProps.notes = document.getElementById('nodeNotes').value;
    nodeProps.pageUrl = document.getElementById('nodePageUrl').value;
    nodeProps.size = parseFloat(document.getElementById('nodeSize').value);

    nodeProps.location = {};

    nodeProps.location.x = parseFloat(document.getElementById('nodeX').value);
    nodeProps.location.y = parseFloat(document.getElementById('nodeY').value);
    nodeProps.location.z = parseFloat(document.getElementById('nodeZ').value);
    nodeProps.location.x = nodeProps.location.x != NaN ? nodeProps.location.x : 0;
    nodeProps.location.y = nodeProps.location.y != NaN ? nodeProps.location.y : 0;
    nodeProps.location.z = nodeProps.location.z != NaN ? nodeProps.location.z : 0;

    nodeProps.location.relativeTo = document.getElementById('nodeRelativeTo').value;

    nodeProps.selected = document.getElementById('nodeSelected').checked;

    return nodeProps;
}

