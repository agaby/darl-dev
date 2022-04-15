var graph;
var demo;
var nodasource;
var interact;
var kgname;
var currentStateId;
var root;
var nodeLookup = {};
var inNoda;


$(async function () {
    inNoda = true;
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
    nodaSource = graph('query ($name: String!){nodaView(graphName: $name)}');

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

    if (!inNoda) {
        alert("This page is intended to be viewed inside the Noda mind-mapping app. Go to https://Noda.io ");
    }
});

async function Build() {
    Clear();
    var res = await nodaSource({ name: kgname });
    root = JSON.parse(res.nodaView);
    $('#msg_input').val(root.initialText);
    var converter = new showdown.Converter();
    var html = converter.makeHtml(root.description);
    $('#kg-description').html(html);
    //now create the network in noda
    if (inNoda) {
        root.nodes.forEach(async function (node) {
            await window.noda.createNode(node);
            nodeLookup[node.uuid] = node;
        });
        root.links.forEach(async function (link) {
            await window.noda.createLink(link);
        });
    }
}
async function Clear() {
    if (inNoda && root !== null && root !== undefined) {
        root.links.forEach(async function (link) {
            await window.noda.deleteLink(link);
        });
        root.nodes.forEach(async function (node) {
            await window.noda.deleteNode(node);
        });
    }
    nodeLookup = {};
    $('#msg_input').val('');
    $('#kg-description').html('');
    root = null;
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
        if (inNoda) {
            res.interactKnowledgeGraph[0].activeNodes.forEach(async function (uuid) {
                var n = nodeLookup[uuid];
                if (n !== undefined) {
                    n.opacity = 1.0;
                    n.sected = true;
                    await window.noda.updateNode(nodeProps);
                }
            });
        }
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


