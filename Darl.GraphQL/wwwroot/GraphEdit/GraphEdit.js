
var allkgmodels;
var graph;
var realkgraphdata;
var realcy;
var virtualcy;
var recognitioncy;
var virtualkgraphdata;
var recognitionkgraphdata;
var virtualeditor;
var recognitioneditor;
var realobjectdata;
var virtualobjectdata;
var recognitionobjectdata;
var realeditorchange;
var virtualeditorchange;
var receditorchange;
var realchanged;
var virtualchanged;
var recchanged;
var mdname;
var createKG;
var savechanges;
var deleterealobject;
var createclonedkg;
var currentNodeId;
var deleterealconnection;
var deleterecognitionobject;
var realConnectiondata;
var createrecognitionconnection;
var createrecognitionobject;
var getlineagesinkgnodes;
var getlineagesinkgconns;
var getlineagesinkgatts;
var createrealobject;
var createrealconnection;
var isvalidlineage;
var getlineagesforword;
var deleterealattribute;
var deleterecognitionattribute;
var updaterealattribute;
var updaterecognitionattribute;
var deletevirtualattribute;
var updatevirtualattribute;
var createrecognitionroot;
var updaterecognitionobject;
var lintCall;
var interact;
var defaultRule;
var getks;
var deletekg;

var recognizedLineage = "adjective:8953";
var currentStateId;

var settingsStorageName = 'thinkbase-settings';
var demo = false;


$(async function () {
    var existing = window.localStorage.getItem(settingsStorageName);
    existing = JSON.parse(existing);
    var url = (existing ? existing.url : "https://darl.dev");
    var key = (existing ? existing.key : "");
    graph = graphql(url + "/graphql");
    var apiKey = findGetParameter("apikey") ;
    mdname = findGetParameter("kgraph");
    if (apiKey !== null)
        graph = graphql(url + "/graphql", { headers: { "Authorization": "Basic " + apiKey } });
    else if (key !== null && key !== "")
        graph = graphql(url + "/graphql", { headers: { "Authorization": "Basic " + key } });
    else {
        demo = true;
        $('#fileHandling').addClass('d-none');
        $('#demohandling').removeClass('d-none');
    }

    allkgmodels = graph(`{ kgraphs { name }}`);
    realkgraphdata = graph('query kgd($model: String!){getRealKGDisplay(graphName: $model){nodes{data{ id label lineage sublineage externalId}} edges{ data{ id label source target}}}}');
    virtualkgraphdata = graph('query vkgd($model: String!){getVirtualKGDisplay(graphName: $model){nodes{data{ id lineage parent}} edges{ data{ id label source target}}}}');
    recognitionkgraphdata = graph('query rkgd($model: String!){getRecognitionKGDisplay(graphName: $model){nodes{data{ id label lineage parent}} edges{ data{ id label source target}}}}');
    realobjectdata = graph('query rod($model: String! $id: String!){getGraphObjectById(graphName: $model id: $id){name lineage subLineage id existence externalId properties {name lineage value type confidence}}}');
    realConnectiondata = graph('query rcd($model: String! $id: String!){getGraphConnectionById(graphName: $model id: $id){name lineage id existence}}');
    virtualobjectdata = graph('query vod($model: String! $lineage: String!){getVirtualObjectByLineage(graphName: $model lineage: $lineage){name lineage id properties {name lineage value type confidence}}}');
    recognitionobjectdata = graph('query recod($model: String! $id: String!){getRecognitionObjectById(graphName: $model id: $id){name lineage id properties {name lineage value type confidence}}}');
    realeditorchange = graph('mutation rec($model: String! $goj: String!){updateGraphObjectFromJSON(graphName: $model graphObjectJSON: $goj ontology: BUILD) {id}}');
    virtualeditorchange = graph('mutation uvg($model: String! $goj: String!){updateVirtualGraphObjectFromJSON(graphName: $model graphObjectJSON: $goj) {id}}');
    receditorchange = graph('mutation rec($model: String! $goj: String!){updateRecognitionObjectFromJSON(graphName: $model graphObjectJSON: $goj) {id}}');
    createKG = graph('mutation createKG($name: String!){createKGraph(name: $name)}');
    savechanges = graph('mutation saveKGraph($name: String!){saveKGraph(name: $name)}');
    deleterealobject = graph('mutation dgo($name: String! $id: String!){deleteGraphObject(graphName: $name id: $id){name}}');
    deleterealconnection = graph('mutation dgc($name: String! $id: String!){deleteGraphConnection(graphName: $name id: $id){name}}');
    deleterecognitionobject = graph('mutation dgc($name: String! $id: String!){deleteRecognitionObject(graphName: $name id: $id){name}}');
    createrecognitionconnection = graph('mutation crc($name: String! $conn: graphConnectionInput!){createRecognitionConnection(name: $name connection: $conn){id}}');
    createrecognitionobject = graph('mutation cro($name: String! $obj: graphObjectInput!){createRecognitionObject(name: $name object: $obj){id}}');
    createrealconnection = graph('mutation crc($name: String! $conn: graphConnectionInput!){createGraphConnection(graphName: $name graphConnection: $conn ontology: BUILD){id}}');
    createrealobject = graph('mutation cro($name: String! $obj: graphObjectInput!){createGraphObject(graphName: $name graphObject: $obj ontology: BUILD){id}}');
    createclonedkg = graph('mutation ckg($name: String! $newname: String!){copyRenamKG(name: $name newName: $newname)}');
    gettypeword = graph('query gtw($lin: String!){getTypeWordForLineage(lineage: $lin)}');
    updateGraphObject = graph('mutation uge($name: String! $obj: graphObjectUpdate!){updateGraphObject(graphName: $name graphObject: $obj){ name }}');
    updateGraphConnection = graph('mutation ugc($name: String! $conn: graphConnectionUpdate!){updateGraphConnection(graphName: $name graphConnection: $conn){ name }}');
    getlineagesinkgnodes = graph('query glkg($name: String!){getLineagesInKG(graphName: $name graphType: NODE){typeWord lineage }}');
    getlineagesinkgconns = graph('query glkg($name: String!){getLineagesInKG(graphName: $name graphType: CONNECTION){typeWord lineage }}');
    getlineagesinkgatts = graph('query glkg($name: String!){getLineagesInKG(graphName: $name graphType: ATTRIBUTE){typeWord lineage }}');
    isvalidlineage = graph('query ivl($lin: String!){isValidLineage(lineage: $lin)}');
    getlineagesforword = graph('query glw($word: String!){getLineagesForWord(word: $word){ typeWord lineage description lineageType}}');
    deleterealattribute = graph('mutation dra($name: String! $id: String! $attLin: String!){deleteGraphObjectAttribute(name: $name id: $id attLineage: $attLin)}');
    deleterecognitionattribute = graph('mutation dra($name: String! $id: String! $attLin: String!){deleteRecognitionObjectAttribute(name: $name id: $id attLineage: $attLin)}');
    deletevirtualattribute = graph('mutation dra($name: String! $lineage: String! $attLin: String!){deleteVirtualObjectAttribute(name: $name lineage: $lineage attLineage: $attLin)}');
    updaterealattribute = graph('mutation uga($name: String! $id: String! $att: graphAttributeInput!){updateGraphObjectAttribute(name: $name id: $id att: $att)}');
    updaterecognitionattribute = graph('mutation uga($name: String! $id: String! $att: graphAttributeInput!){updateRecognitionObjectAttribute(name: $name id: $id att: $att)}');
    updatevirtualattribute = graph('mutation uga($name: String! $lineage: String! $att: graphAttributeInput!){updateVirtualObjectAttribute(name: $name lineage: $lineage att: $att)}');
    createrecognitionroot = graph('mutation crr($name: String! $lineage: String!){createRecognitionRoot(name: $name lineage: $lineage ){id}}');
    updaterecognitionobject = graph('mutation uro($name: String! $obj: graphObjectUpdate!){updateRecognitionObject(name: $name object: $obj){id}}');
    lintCall = graph('query lint($darl: String!){  lintDarlMeta(darl: $darl){ column_no_start column_no_stop line_no message severity }}');
    interact = graph('query int($name: String! $ksid: String! $text:  String!){interactKnowledgeGraph(kgModelName: $name conversationId: $ksid conversationData: { dataType: textual name: "" value: $text }){ darl reference response{dataType name value categories{name value }}}}');
    defaultRule = graph('query dr($lineage: String!){getSuggestedRuleset(lineage: $lineage)}');
    getks = graph('query gks($id: String!){getKnowledgeState(id: $id external: true){userId knowledgeGraphName data {name value {name lineage value confidence type }}}}')
    deletekg = graph('mutation dkg($name: String!){deleteKG(name: $name)}');

    if (mdname !== null) {
        $('#fileHandling').addClass('d-none');
        await loadGraphs();
    }
    else {
        await updateDropdown();

        $('#kgmodel-dropdown').on('change', async function () {
            mdname = this.value;
            //getGraphData
            await loadGraphs();
        });
        $('#kgdemo-dropdown').on('change', async function () {
            mdname = this.value;
            //getGraphData
            await loadGraphs();
        });
    }

    $('#kg-create').click(async function () {
        mdname = $('#kg-input').val();
        try {
            var res = await createKG({ name: mdname });
            await updateDropdown();
            alert(mdname + " created");
        }
        catch (err) {
            HandleError(err);
        }
    });

    $('#kg-copy').click(async function () {
        if (!mdname || mdname === "") {
            alert("You have to select a knowledge graph to copy");
            return;
        }
        $.MessageBox({
            input: {
                newName: {
                    type: "text",
                    label: "New KG name"
                },
                capt1: {
                    type: "caption",
                    message: "Existing KGs with the same name will be overwritten."

                }
            },
            message: "Copy to new Knowledge Graph",
            buttonDone: "Copy",
            buttonFail: "Cancel",
            filterDone: function (data) {
                if (data.newName === "") return "You have to give a name for the new knowledge graph";
            }
            }).done(async function (data) {
            try {
                await createclonedkg({ name: mdname, newname: data.newName });
                alert(mdname + " copied to " + data.newName + ".");
                await updateDropdown();
            }
            catch (err) {
                HandleError(err);
            }});
    });

    $('#kg-delete').click(async function () {
        if (!mdname) {
            $.MessageBox("No KG is selected.");
            return;
        }
        $.MessageBox({
            buttonDone: "Yes",
            buttonFail: "No",
            message: "Delete this Knowledge Graph?"
        }).done(async function () {
            try {
                await deletekg({ name: mdname });
                await updateDropdown();
            }
            catch (err) {
                HandleError(err);
            }
        })
    });

    $('#kg-save').click(async function () {
        try {
            await savechanges({ name: mdname });
//            $('#kg-save').prop('disabled', true);
            alert(mdname + " saved");
        }
        catch (err) {
            HandleError(err);
        }
    });

    $('#settings').click(function () {
        var existing = window.localStorage.getItem(settingsStorageName);
        existing = JSON.parse(existing)
        var url = (existing ? existing.url : "https://darl.dev");
        var key = (existing ? existing.key : "");
        $.MessageBox({
            input: {
                url: {
                    type: "text",
                    label: "The ThinkBase source",
                    defaultValue: url 
                },
                key: {
                    type: "password",
                    label: "Your ThinkBase API key",
                    defaultValue: key 
                }
            },
            message: "Change the settings",
            buttonDone: "Change",
            buttonFail: "Cancel",
            filterDone: function (data) {
                if (data.url === "" ) return "You must supply a url.";
            }
        }).done(async function (data) {
            window.localStorage.setItem(settingsStorageName, JSON.stringify(data));
            //update 
        });
    });

});

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

async function updateDropdown() {
    var dropdown = demo ? $('#kgdemo-dropdown') : $('#kgmodel-dropdown');
    dropdown.empty();
    dropdown.append('<option selected="true" disabled>Choose a Knowledge Graph.</option>');
    dropdown.prop('selectedIndex', 0);
    try {
        const rs = await allkgmodels();
        $.each(rs.kgraphs, function (key, entry) {
            dropdown.append($('<option class="dropdown-item"></option>').attr('value', entry.name).text(entry.name));
        });
    }
    catch (err) {
        HandleError(err);
    }

}

 function updateStateDropdown() {
     const dropdown = $('#conv-recent-dropdown');
     var idList = [];
     //get local set of state ids
     const storageName = mdname + '_knowledge_states';
     var existing = window.localStorage.getItem(storageName);
     if (existing) {
         idList = JSON.parse(existing);
     }
     dropdown.empty();
     dropdown.append('<option selected="true" disabled>Choose a knowledge state</option>');
     dropdown.prop('selectedIndex', 0);
     $.each(idList, function (key, entry) {
        dropdown.append($('<option class="dropdown-item"></option>').attr('value', entry).text(entry));
    });
}

async function loadGraphs() {
    try {
        $('#real-header').removeClass('d-none');
        var loading = document.getElementById('loading');
        loading.classList.remove('loaded');
        updateStateDropdown();
        var realdata = await realkgraphdata({ model: mdname });
        var recdata = await recognitionkgraphdata({ model: mdname });
        //instantiate graphs here
        realcy = cytoscape({
            container: $('#realgraph'),
            elements: realdata.getRealKGDisplay,
            style: [ // the stylesheet for the graph
                {
                    selector: 'node',
                    style: {
                        'background-color': '#11479e',
                        'label': 'data(externalId)'
                    }
                },
                {
                    selector: 'node:selected',
                    style: {
                        'background-color': '#1010ff',
                        'label': 'data(externalId)'
                    }
                },
                {
                    selector: 'edge',
                    style: {
                        'width': 3,
                        'line-color': '#9dbaea',
                        'target-arrow-color': '#9dbaea',
                        'target-arrow-shape': 'triangle',
                        'curve-style': 'bezier'
                    }
                },
                {
                    selector: '.eh-handle',
                    style: {
                        'background-color': 'red',
                        'width': 12,
                        'height': 12,
                        'shape': 'ellipse',
                        'overlay-opacity': 0,
                        'border-width': 12, // makes the handle easier to hit
                        'border-opacity': 0
                    }
                },

                {
                    selector: '.eh-hover',
                    style: {
                        'background-color': 'red'
                    }
                },

                {
                    selector: '.eh-source',
                    style: {
                        'border-width': 2,
                        'border-color': 'red'
                    }
                },

                {
                    selector: '.eh-target',
                    style: {
                        'border-width': 2,
                        'border-color': 'red'
                    }
                },

                {
                    selector: '.eh-preview, .eh-ghost-edge',
                    style: {
                        'background-color': 'red',
                        'line-color': 'red',
                        'target-arrow-color': 'red',
                        'source-arrow-color': 'red'
                    }
                },

                {
                    selector: '.eh-ghost-edge.eh-preview-active',
                    style: {
                        'opacity': 0
                    }
                }
            ],

            layout: {

                name: 'cose',
                idealEdgeLength: 100,
                nodeOverlap: 20,
                refresh: 20,
                fit: true,
                padding: 30,
                randomize: false,
                componentSpacing: 100,
                nodeRepulsion: 400000,
                edgeElasticity: 100,
                nestingFactor: 5,
                gravity: 80,
                numIter: 1000,
                initialTemp: 200,
                coolingFactor: 0.95,
                minTemp: 1.0
            }

        });
        realcy.cxtmenu({
            selector: 'node',
            commands: [
                {
                    content: '<span class="fa fa-trash fa-2x"></span>',
                    select: async function (ele) {
                        if (ele.hasClass("eh-handle")) {
                            ele = ele.data("mainNode");
                        }
                        $.MessageBox({
                            buttonDone: "Yes",
                            buttonFail: "No",
                            message: "Are you sure you want to delete this node?"
                        }).done(async function (data) {
                            console.log(data);
                            await deleterealobject({ name: mdname, id: ele.id() });
                            realcy.remove(ele);
                        });
                    }
                },
                {
                    content: '<span class="fa fa-info fa-2x"></span>',
                    select: async function (ele) {
                        ShowInfo("md/thinkbase/real_node.md");
                    }
                },

                {
                    content: '<span class="fa fa-calendar fa-2x"></span>',
                    select: async function (ele) {
                        console.log('existence');
                        if (ele.hasClass("eh-handle")) {
                            ele = ele.data("mainNode");
                        }
                        var obj = await realobjectdata({ model: mdname, id: ele.id() });
                        if (obj) {
                            if (obj.getGraphObjectById.existence) {
                                for (var n = 0; n < obj.getGraphObjectById.existence.length && n < 4; n++) {
                                    if (n == 0)
                                        $('#time0').val(obj.existence[n]);
                                    else if (n == 1)
                                        $('#time1').val(obj.existence[n]);
                                    else if (n == 2)
                                        $('#time2').val(obj.existence[n]);
                                    else if (n == 3)
                                        $('#time3').val(obj.existence[n]);
                                }
                            }
                            $.MessageBox({
                                input: {
                                    date1: {
                                        type: "date",
                                        label: "Date 1"
                                    },
                                    time1: {
                                        type: "time",
                                        label: "Time 1"
                                    },
                                    date2: {
                                        type: "date",
                                        label: "Date 2"
                                    },
                                    time2: {
                                        type: "time",
                                        label: "Time 2"
                                    },
                                    date3: {
                                        type: "date",
                                        label: "Date 3"
                                    },
                                    time3: {
                                        type: "time",
                                        label: "Time 3"
                                    },
                                    date4: {
                                        type: "date",
                                        label: "Date 4"
                                    },
                                    time4: {
                                        type: "time",
                                        label: "Time 4"
                                    },
                                    dummy_caption: {
                                        type: "caption",
                                        message: "One value constitutes an event, two an interval, <br/>three a fuzzy event and four a fuzzy interval.<br/>Entries will be sorted in time-order before use."
                                    }
                                },
                                message: "Set or change the times of this element's existence",
                                buttonDone: "Change",
                                buttonFail: "Cancel"
                            }).done(async function (data) {
                                var times = new Array();
                                if (data.date1) {
                                    if (data.time1) {
                                        times.push(date1 + time1);
                                    }
                                    else {
                                        times.push(date1);
                                    }
                                }
                                if (data.date2) {
                                    if (data.time2) {
                                        times.push(date2 + time2);
                                    }
                                    else {
                                        times.push(date2);
                                    }
                                }
                                if (data.date3) {
                                    if (data.time3) {
                                        times.push(date3 + time3);
                                    }
                                    else {
                                        times.push(date3);
                                    }
                                }
                                if (data.date4) {
                                    if (data.time4) {
                                        times.push(date4 + time4);
                                    }
                                    else {
                                        times.push(date4);
                                    }
                                }
                                await updateGraphObject({ name: mdname, conn: { id: ele.id(), existence: times } });
                                console.log(data);
                            });
                        }
                    }
                },

                {
                    content: '<span class="fa fa-arrow-up fa-2x"></span>',
                    select: async function (ele) {
                        try {
                            console.log('external id');
                            if (ele.hasClass("eh-handle")) {
                                ele = ele.data("mainNode");
                            }
                            var id = ele.id();
                            var obj = await realobjectdata({ model: mdname, id: id });
                            if (obj) {
                                $.MessageBox({
                                    input: obj.getGraphObjectById.externalId,
                                    message: "Edit the external Id",
                                    buttonDone: "Change",
                                    buttonFail: "Cancel",
                                    filterDone: function (data) {
                                        if (data === "") return "ExternalId must have a value!";
                                    }
                                }).done(async function (data) {
                                    if (obj.getGraphObjectById.externalId !== data) {
                                        await updateGraphObject({ name: mdname, obj: { id: ele.id(), externalId: data } });
                                        ele.data('externalId', externalId);
                                        console.log(data);
                                    }
                                });
                            }
                        }
                        catch (err) {
                            HandleError(err);
                        }
                    }
                },

                {
                    content: '<span class="fa fa-user fa-2x"></span>',
                    select: async function (ele) {
                        try {
                            console.log('name');
                            if (ele.hasClass("eh-handle")) {
                                ele = ele.data("mainNode");
                            }
                            var id = ele.id();
                            var obj = await realobjectdata({ model: mdname, id: id });
                            if (obj) {
                                $.MessageBox({
                                    input: obj.getGraphObjectById.name,
                                    message: "Edit the name",
                                    buttonDone: "Change",
                                    buttonFail: "Cancel",
                                    filterDone: function (data) {
                                        if (data === "") return "Name must have a value!";
                                    }
                                }).done(async function (data) {
                                    if (obj.getGraphObjectById.name !== data) {
                                        await updateGraphObject({ name: mdname, obj: { id: ele.id(), name: data } });
                                        ele.data('label', data);
                                        console.log(data);
                                    }
                                });
                            }
                        }
                        catch (err) {
                            HandleError(err);
                        }
                    }
                },

                {
                    content: '<span class="fa fa-tasks fa-2x"></span>',
                    select: async function (ele) {
                        console.log('attributes');
                        if (ele.hasClass("eh-handle")) {
                            ele = ele.data("mainNode");
                        }
                        var id = ele.id();
                        await EditRealAttributes(id);
                    }
                },
                {
                    content: '<span class="fa fa-tree fa-2x"></span>',
                    select: async function (ele) {
                        console.log('lineage');
                        if (ele.hasClass("eh-handle")) {
                            ele = ele.data("mainNode");
                        }
                        try {
                            var obj = await realobjectdata({ model: mdname, id: ele.id() });
                            if (obj) {
                                var lin = obj.getGraphObjectById.lineage;
                                var typeword = await gettypeword({ lin: lin });
                                var subTypeword = "";
                                var sublin = obj.getGraphObjectById.subLineage;
                                if (sublin) {
                                    subTypeword = await gettypeword({ lin: sublin }).getTypeWordForLineage;
                                }
                                $.MessageBox({
                                    input: {
                                        lineage:
                                        {
                                            type: "caption",
                                            message: lin
                                        },
                                        typeword:
                                        {
                                            type: "caption",
                                            message: typeword.getTypeWordForLineage
                                        },
                                        sublineage:
                                        {
                                            type: "caption",
                                            message: sublin
                                        },
                                        subtypeword:
                                        {
                                            type: "caption",
                                            message: subTypeword
                                        }
                                    },
                                    message: "The lineage"
                                }).done(function (data) {
                                    console.log(data);
                                });
                            }
                        }
                        catch (err) {
                            HandleError(err);
                        }
                    }
                }
            ]
        });
        realcy.cxtmenu({
            selector: 'edge',
            commands: [
                {
                    content: '<span class="fa fa-trash fa-2x"></span>',
                    select: async function (ele) {
                        $.MessageBox({
                            buttonDone: "Yes",
                            buttonFail: "No",
                            message: "Are you sure you want to delete this node?"
                        }).done(async function (data) {
                            try {
                                console.log(data);
                                await deleterealconnection({ name: mdname, id: ele.id() });
                                realcy.remove(ele);
                            }
                            catch (err) {
                                HandleError(err);
                            }
                        });
                    }
                },

                {
                    content: '<span class="fa fa-calendar fa-2x"></span>',
                    select: async function (ele) {
                        console.log('existence');
                        var obj = await realConnectiondata({ model: mdname, id: ele.id() });
                        if (obj) {
                            if (obj.getGraphConnectionById.existence) {
                                for (var n = 0; n < obj.getGraphObjectById.existence.length && n < 4; n++) {
                                    if (n === 0)
                                        $('#time0').val(obj.existence[n]);
                                    else if (n === 1)
                                        $('#time1').val(obj.existence[n]);
                                    else if (n === 2)
                                        $('#time2').val(obj.existence[n]);
                                    else if (n === 3)
                                        $('#time3').val(obj.existence[n]);
                                }
                            }
                            $.MessageBox({
                                input: {
                                    date1: {
                                        type: "date",
                                        label: "Date 1"
                                    },
                                    time1: {
                                        type: "time",
                                        label: "Time 1"
                                    },
                                    date2: {
                                        type: "date",
                                        label: "Date 2"
                                    },
                                    time2: {
                                        type: "time",
                                        label: "Time 2"
                                    },
                                    date3: {
                                        type: "date",
                                        label: "Date 3"
                                    },
                                    time3: {
                                        type: "time",
                                        label: "Time 3"
                                    },
                                    date4: {
                                        type: "date",
                                        label: "Date 4"
                                    },
                                    time4: {
                                        type: "time",
                                        label: "Time 4"
                                    },
                                    dummy_caption: {
                                        type: "caption",
                                        message: "One value constitutes an event, two an interval, <br/>three a fuzzy event and four a fuzzy interval.<br/>Entries will be sorted in time-order before use."
                                    }
                                },
                                message: "Set or change the times of this element's existence",
                                buttonDone: "Change",
                                buttonFail: "Cancel"
                            }).done(async function (data) {
                                var times = new Array();
                                if (data.date1) {
                                    if (data.time1) {
                                        times.push(date1 + time1);
                                    }
                                    else {
                                        times.push(date1);
                                    }
                                }
                                if (data.date2) {
                                    if (data.time2) {
                                        times.push(date2 + time2);
                                    }
                                    else {
                                        times.push(date2);
                                    }
                                }
                                if (data.date3) {
                                    if (data.time3) {
                                        times.push(date3 + time3);
                                    }
                                    else {
                                        times.push(date3);
                                    }
                                }
                                if (data.date4) {
                                    if (data.time4) {
                                        times.push(date4 + time4);
                                    }
                                    else {
                                        times.push(date4);
                                    }
                                }
                                await updateGraphConnection({ name: mdname, conn: { id: ele.id(), existence: times } });
                                console.log(data);
                            });
                        }
                    }
                },
                {
                    content: '<span class="fa fa-info fa-2x"></span>',
                    select: async function (ele) {
                        ShowInfo("md/thinkbase/real_Connection.md");
                    }
                },

                {
                    content: '<span class="fa fa-user fa-2x"></span>',
                    select: async function (ele) {
                        console.log('name');
                        try {
                            var id = ele.id();
                            var obj = await realConnectiondata({ model: mdname, id: id });
                            if (obj) {
                                $.MessageBox({
                                    input: obj.getGraphConnectionById.name,
                                    message: "Edit the name",
                                    buttonDone: "Change",
                                    buttonFail: "Cancel",
                                    filterDone: function (data) {
                                        if (data === "") return "Name must have a value";
                                    }
                                }).done(async function (data) {
                                    try {
                                        if (obj.getGraphConnectionById.name !== data) {
                                            await updateGraphConnection({ name: mdname, conn: { id: ele.id(), name: data } });
                                            ele.data('label', name);
                                            console.log(data);
                                        }
                                    }
                                    catch (err) {
                                        HandleError(err);
                                    }
                                });
                            }
                        }
                        catch (err) {
                            HandleError(err);
                        }
                    }
                },
                {
                    content: '<span class="fa fa-tree fa-2x"></span>',
                    select: async function (ele) {
                        console.log('lineage');
                        var obj = await realConnectiondata({ model: mdname, id: ele.id() });
                        if (obj) {
                            var lin = obj.getGraphConnectionById.lineage;
                            var typeword = await gettypeword({ lin: lin });
                            $.MessageBox({
                                input: {
                                    lineage:
                                    {
                                        type: "caption",
                                        message: lin
                                    },
                                    typeword:
                                    {
                                        type: "caption",
                                        message: typeword.getTypeWordForLineage
                                    }
                                },
                                message: "The lineage"
                            }).done(function (data) {
                                console.log(data);
                            });
                        }
                    }
                }
            ]
        });
        realcy.edgehandles({
            snap: true
        });
        realcy.on('tap', async function (evt) {
            var node = evt.target;
            if (node === realcy) {
                try {
                    var nodelins = await getlineagesinkgnodes({ name: mdname });
                    var obj = {};
                    $.each(nodelins.getLineagesInKG, function (i, item) {
                        obj[item.lineage] = item.typeWord;
                    });
                    $.MessageBox({
                        input: {
                            name:
                            {
                                type: "text",
                                label: "Name"
                            },
                            externalId:
                            {
                                type: "text",
                                label: "External id"
                            },
                            sep_caption: {
                                type: "caption",
                                message: "select an existing lineage <br/> or enter a new one."
                            },
                            existinglineage: {
                                label: "existing lineages",
                                type: "select",
                                options: obj
                            },
                            newlineage: {
                                type: "text",
                                label: "new lineage"
                            },
                            sep_subcaption: {
                                type: "caption",
                                message: "optionally select an existing lineage for the sub-lineage<br/> or enter a new one."
                            },
                            existingsublineage: {
                                label: "existing lineages",
                                type: "select",
                                options: obj
                            },
                            newsublineage: {
                                type: "text",
                                label: "new sub-lineage"
                            }
                        },
                        buttonDone: "Add",
                        buttonFail: "Cancel",
                        message: "Add a new node to the network.",
                        filterDone: function (data) {
                            if (data.name === "") return "Please supply a name";
                            if (data.externalId === "") return "Please supply an externalId";
                            if (data.newLineage === "" && data.existinglineage === "") return "Choose an existing lineage or add a new word or lineage.";
                        }

                    }).done(async function (data) {
                        console.log('add node ' + data);
                        //create new object here
                        var lin;
                        var sublin;
                        var name = data.name;
                        var externalId = data.externalId;
                        if (data.existinglineage ) {//no lookup needed
                            lin = data.existinglineage;        
                        }
                        if (data.existingsublineage) {
                            sublin = data.existingsublineage;
                        }               
                        if (!lin || (!sublin && data.newsublineage !== ""))
                        {
                            //check if new lineage and new sublineage are valid lineages
                            var ivl = await isvalidlineage({ lin: data.newlineage });
                            if (ivl.isValidLineage) {
                                lin = data.newlineage;
                            }
                            if (data.newsublineage !== "") {
                                ivl = await isvalidlineage({ lin: data.newsublineage });
                                if (ivl.isValidLineage) {
                                    sublin = data.newsublineage;
                                }
                            }
                            //check if either is null
                            if (!lin || !sublin) {
                                var lineages = await getlineagesforword({ word: data.newlineage });
                                var obj = {};
                                $.each(lineages.getLineagesForWord, function (i, item) {
                                    obj[item.lineage] = item.typeWord + ": " + item.description;
                                });
                                var sublineages = await getlineagesforword({ word: data.newsublineage });
                                var subobj = {};
                                $.each(sublineages.getLineagesForWord, function (i, item) {
                                    subobj[item.lineage] = item.typeWord + ": " + item.description;
                                });
                                if (!lin && !sublin) {
                                    $.MessageBox({
                                        input: {
                                            lin: {
                                                label: "Possible lineages",
                                                type: "select",
                                                options: obj
                                            },
                                            sublin: {
                                                label: "Possible sub-lineages",
                                                type: "select",
                                                options: subobj
                                            }
                                        },
                                        buttonDone: "Select",
                                        buttonFail: "Cancel",
                                        message: "Choose primary and sub-lineage for these words.",
                                        filterDone: function (data) {
                                            if (data.lin === "" || data.sublin === "") return "Select lineages.";
                                        }
                                    }).done(async function (data) {
                                        sublin = data.sublin;
                                        lin = data.lin;
                                        await CreateNode(evt, name, externalId, lin, sublin);
                                    });
                                }
                                else if (!sublin) {
                                    $.MessageBox({
                                        input: {
                                            lin: {
                                                label: "Possible sub-lineages",
                                                type: "select",
                                                options: subobj
                                            }
                                        },
                                        buttonDone: "Select",
                                        buttonFail: "Cancel",
                                        message: "Choose a lineage for this word.",
                                        filterDone: function (data) {
                                            if (data.lin === "") return "Select a lineage.";
                                        }
                                    }).done(async function (data) {
                                        sublin = data.lin;
                                        await CreateNode(evt, name, externalId, lin, sublin);
                                    });
                                }
                                else {
                                    $.MessageBox({
                                        input: {
                                            lin: {
                                                label: "Possible lineages",
                                                type: "select",
                                                options: obj
                                            }
                                        },
                                        buttonDone: "Select",
                                        buttonFail: "Cancel",
                                        message: "Choose a lineage for this word.",
                                        filterDone: function (data) {
                                            if (data.lin === "") return "Select a lineage.";
                                        }
                                    }).done(async function (data) {
                                        lin = data.lin;
                                        await CreateNode(evt, name, externalId, lin, sublin);
                                    });
                                }
                            }
                            else {
                                await CreateNode(evt, name, externalId, lin, sublin);
                            }
                        }
                    });
                }
                catch (err) {
                    HandleError(err);
                }
            }
            else {
                if (node.data('sublineage')) {
                    const ref = '#' + node.data('lineage').replace(/,/g, '-').replace(/:/g, '-') + '+' + node.data('sublineage').replace(/,/g, '-').replace(/:/g, '-');
                    const virt = virtualcy.$(ref);
                    virt.select();
                }
                else {
                    const ref = '#' + node.data('lineage').replace(/,/g, '-').replace(/:/g, '-');
                    const virt = virtualcy.$(ref);
                    virt.select();
                }
            }
        });
        realcy.on('layoutstop', function (event) {
            var loading = document.getElementById('loading');
            loading.classList.add('loaded');
        });
        realcy.on('ehcomplete', async function (event, sourceNode, targetNode, addedEles) {
            var connlins = await getlineagesinkgconns({ name: mdname });
            var obj = {};
            $.each(connlins.getLineagesInKG, function (i, item) {
                obj[item.lineage] = item.typeWord;
            });
            $.MessageBox({
                input: {
                    name:
                    {
                        type: "text",
                        label: "Name"
                    },
                    sep_caption: {
                        type: "caption",
                        message: "select an existing lineage <br/> or enter a new one."
                    },
                    existinglineage: {
                        label: "existing lineages",
                        type: "select",
                        options: obj
                    },
                    newlineage: {
                        type: "text",
                        label: "new lineage"
                    }
                },
                buttonDone: "Add",
                buttonFail: "Cancel",
                message: "Add a new edge to the network.",
                filterDone: function (data) {
                    if (data.name === "") return "Please supply a name";
                    if (data.newLineage === "" && data.existinglineage === "") return "Choose an existing lineage or add a new word or lineage.";
                }

            }).done(async function (data) {
                var lin;
                if (data.existinglineage) {
                    lin = data.existinglineage;
                    await CreateConnection(addedEles, sourceNode, targetNode, data.name, lin);
                }
                else {
                    try {
                        lin = data.newlineage;
                        var name = data.name;
                        //check if valid lineage
                        var ivl = await isvalidlineage({ lin: lin });
                        if (!ivl.isValidLineage) {
                            //Create a messageBox to offer alternatives
                            var lineages = await getlineagesforword({ word: lin });
                            var obj = {};
                            $.each(lineages.getLineagesForWord, function (i, item) {
                                obj[item.lineage] = item.typeWord + ": " + item.description;
                            });
                            $.MessageBox({
                                input: {
                                    lin: {
                                        label: "Possible lineages",
                                        type: "select",
                                        options: obj
                                    }
                                },
                                buttonDone: "Select",
                                buttonFail: "Cancel",
                                message: "Choose a lineage for this word.",
                                filterDone: function (data) {
                                    if (data.lin === "") return "Select a lineage.";
                                }
                            }).done(async function (data) {
                                lin = data.lin;
                                await CreateConnection(addedEles, sourceNode, targetNode, name, lin);
                            }).fail(function (data) {
                                realcy.remove(addedEles);
                            });
                        }
                        else {
                            await CreateConnection(addesEles, sourceNode, targetNode, name, lin);
                        }
                    }
                    catch (err) {
                        HandleError(err);
                        realcy.remove(addedEles);
                    }
                }
                console.log('add edge ' + data);
            }).fail(function (data) {
                realcy.remove(addedEles);
            });
        });
        realcy.on('add', function (event) {
            node = event.target;
            if (!node.data('externalId')) {
                //realcy.remove(node);
                console.log(node.toString());
            }
        });

        await LoadVirtualGraph();

        recognitioncy = cytoscape({
            container: $('#recognitiongraph'),
            elements: recdata.getRecognitionKGDisplay,
            style: [ // the stylesheet for the graph
                {
                    selector: 'node',
                    style: {
                        'background-color': '#666',
                        'label': 'data(lineage)'
                    }
                },
                {
                    selector: 'node:selected',
                    style: {
                        'background-color': '#1010ff',
                        'label': 'data(externalId)'
                    }
                },
                {
                    selector: ':parent',
                    css: {
                        'text-valign': 'top',
                        'text-halign': 'center',
                        'background-opacity': 0.333
                    }
                },
                {
                    selector: 'edge',
                    style: {
                        'width': 3,
                        'line-color': '#ccc',
                        'target-arrow-color': '#ccc',
                        'target-arrow-shape': 'triangle',
                        'curve-style': 'bezier',
                        'label': 'data(label)'
                    }
                },
                {
                    selector: '.eh-handle',
                    style: {
                        'background-color': 'red',
                        'width': 12,
                        'height': 12,
                        'shape': 'ellipse',
                        'overlay-opacity': 0,
                        'border-width': 12, // makes the handle easier to hit
                        'border-opacity': 0
                    }
                },

                {
                    selector: '.eh-hover',
                    style: {
                        'background-color': 'red'
                    }
                },

                {
                    selector: '.eh-source',
                    style: {
                        'border-width': 2,
                        'border-color': 'red'
                    }
                },

                {
                    selector: '.eh-target',
                    style: {
                        'border-width': 2,
                        'border-color': 'red'
                    }
                },

                {
                    selector: '.eh-preview, .eh-ghost-edge',
                    style: {
                        'background-color': 'red',
                        'line-color': 'red',
                        'target-arrow-color': 'red',
                        'source-arrow-color': 'red'
                    }
                },

                {
                    selector: '.eh-ghost-edge.eh-preview-active',
                    style: {
                        'opacity': 0
                    }
                }

            ],

            layout: {

                name: 'dagre'
            }
        });
        recognitioncy.cxtmenu({
            selector: 'node',
            commands: [
                {
                    content: '<span class="fa fa-trash fa-2x"></span>',
                    select: async function (ele) {
                        if (ele.hasClass("eh-handle")) {
                            ele = ele.data("mainNode");
                        }
                        $.MessageBox({
                            buttonDone: "Yes",
                            buttonFail: "No",
                            message: "Are you sure you want to delete this node?"
                        }).done(async function (data) {
                            try {
                                console.log(data);
                                await deleterecognitionobject({ name: mdname, id: ele.id() });
                                realcy.remove(ele);
                            }
                            catch (err) {
                                HandleError(err);
                            }
                        });
                    }
                },
                {
                    content: '<span class="fa fa-info fa-2x"></span>',
                    select: async function (ele) {
                        ShowInfo("md/thinkbase/recognition_node.md");
                    }
                },
                {
                    content: '<span class="fa fa-tasks fa-2x"></span>',
                    select: async function (ele) {
                        if (ele.hasClass("eh-handle")) {
                            ele = ele.data("mainNode");
                        }
                        console.log('attributes');
                        var id = ele.id();
                        await EditRecognitionAttributes(id);
                    }
                },
                {
                    content: '<span class="fa fa-tree fa-2x"></span>',
                    select: async function (ele) {
                        if (ele.hasClass("eh-handle")) {
                            ele = ele.data("mainNode");
                        }
                        console.log('lineage');
                        var obj = await recognitionobjectdata({ model: mdname, id: ele.id() });
                        if (obj) {
                            var lin = obj.getRecognitionObjectById.lineage;
                            $.MessageBox({
                                input: {
                                    word:
                                    {
                                        type: "text",
                                        label: "Word",
                                        defaultValue: lin
                                    },
                                    convLin:
                                    {
                                        type: "checkbox",
                                        label: "Suggest lineage?"
                                    },
                                },
                                buttonDone: "Change",
                                buttonFail: "Cancel",
                                message: "The word to recognise",
                                filterDone: function (data) {
                                    if (data.word === "") return "Supply a word.";
                                }
                            }).done(async function (data) {
                                try {
                                    console.log('add node ' + data);
                                    var newObj = {id: obj.id, lineage: data.word, name: data.word};
                                    if (data.convLin) {
                                        var lineages = await getlineagesforword({ word: data.word });
                                        var lins = {};
                                        $.each(lineages.getLineagesForWord, function (i, item) {
                                            lins[item.lineage] = item.typeWord + ": " + item.description;
                                        });
                                        $.MessageBox({
                                            input: {
                                                lin: {
                                                    label: "Possible lineages",
                                                    type: "select",
                                                    options: lins
                                                }
                                            },
                                            buttonDone: "Select",
                                            buttonFail: "Cancel",
                                            message: "Choose a lineage for this word.",
                                            filterDone: function (data) {
                                                if (data.lin === "") return "Select a lineage or cancel.";
                                            }
                                        }).done(async function (data) {
                                            newObj.lineage = data.lin;
                                            await updaterecognitionobject({name: mdname, obj: newObj});
                                        });
                                    }
                                    else {
                                        await updaterecognitionobject({ name: mdname, obj: newObj });
                                    }
                                }
                                catch (err) {
                                    HandleError(err);
                                }
                            });
                        }
                    }
                }
            ]
        });
        recognitioncy.edgehandles({
            snap: true
        });
        recognitioncy.on('tap', async function (evt) {
            var node = evt.target;
            if (node === recognitioncy) {
                $.MessageBox({
                    input: {
                        word:
                        {
                            type: "text",
                            label: "Word"
                        },
                        convLin:
                        {
                            type: "checkbox",
                            label: "Suggest lineage?"
                        },
                    },
                    buttonDone: "Add",
                    buttonFail: "Cancel",
                    message: "Add a word to the recognition tree.",
                    filterDone: function (data) {
                        if (data.word === "") return "Supply a word";
                    }
                }).done(async function (data) {
                    try {
                        console.log('add node ' + data);
                        if (data.convLin) {
                            var lineages = await getlineagesforword({ word: data.word });
                            var obj = {};
                            $.each(lineages.getLineagesForWord, function (i, item) {
                                obj[item.lineage] = item.typeWord + ": " + item.description;
                            });
                            $.MessageBox({
                                input: {
                                    lin: {
                                        label: "Possible lineages",
                                        type: "select",
                                        options: obj
                                    }
                                },
                                buttonDone: "Select",
                                buttonFail: "Cancel",
                                message: "Choose a lineage for this word.",
                                filterDone: function (data) {
                                    if (data.lin === "") return "Select a lineage or cancel.";
                                }
                           }).done(async function (data) {
                                await CreateRecognitionNode(evt, data.lin);
                            });
                        }
                        else {
                            await CreateRecognitionNode(evt, data.word);
                        }
                    }
                    catch (err) {
                        HandleError(err);
                    }
                });

            }
        });

        recognitioncy.on('ehcomplete', async function (event, sourceNode, targetNode, addedEles) {
            var res = await createrecognitionconnection({ name: mdname, conn: { startId: sourceNode.id(), endId: targetNode.id(), lineage: "", name: "" } })
            if (!res.createRecognitionConnection) { //failed, delete connection
                recognitioncy.remove(addedEles);
            }
        });
        recognitioncy.on('add', function (event) {
            node = event.target;
            if (!node.data('lineage')) {
                //realcy.remove(node);
                console.log(node);
            }
        });



        realchanged = true;
        virtualchanged = true;
        recchanged = true;

        $('#real-find').click(function () {
            $.MessageBox({
                input: true,
                message: "ExternalId to search for:",
                buttonDone: "Find",
                buttonFail: "Cancel",
            }).done(function (data) {
                if ($.trim(data)) {
                    var nodes = realcy.nodes().filter(function (element, i) {
                        return element.data('externalId') === $.trim(data);
                    });
                    realcy.fit(nodes, 300);
                    nodes.emit('tap');
                }
            });
        });

        $('#real-help').click(function () {
            ShowInfo("md/thinkbase/real_view.md");
        });

        $('#real-time').click(function () {
            $.MessageBox({
                input: true,
                message: "ExternalId to search for:",
                buttonDone: "Find",
                buttonFail: "Cancel",
            }).done(function (data) {
                if ($.trim(data)) {
                    var nodes = realcy.nodes().filter(function (element, i) {
                        return element.data('externalId') === $.trim(data);
                    });
                    realcy.fit(nodes, 300);
                    nodes.emit('tap');
                }
            });
        });

        $('#real-fit').click(function () { realcy.fit(); });
        $('#virtual-fit').click(function () { virtualcy.fit(); });
        $('#rec-fit').click(function () { recognitioncy.fit(); });
        $('#rec-addroot').click(async function () {
            $.MessageBox({
                input: {
                    lin: {
                        label: "Possible lineages",
                        type: "select",
                        options: {
                            "default:": "default",
                            "navigation:": "navigation",
                        }
                    }
                },
                buttonDone: "Select",
                buttonFail: "Cancel",
                message: "Choose a root to add.",
                filterDone: function (data) {
                    if (data.lin === "") return "Select a root or cancel.";
                }

            }).done(async function (data) {
                try {
                    var created = await createrecognitionroot({ name: mdname, lineage: data.lin });
                    recognitioncy.add({
                        group: 'nodes',
                        data: { label: data.lin, id: created.createRecognitionRoot.id, lineage: data.lin },
                        position: {
                            x: 100,
                            y: 100
                        }
                    });
                    var layout = recognitioncy.layout({
                        name: 'dagre'
                    });
                    layout.run();

                }
                catch (err) {
                    HandleError(err);
                }
            });
        });

        $('#conv-newstate').click(async function () {
            const storageName = mdname + '_knowledge_states';
            currentStateId = uuidv4();
            var idList = [];
            //get local set of state ids
            var existing = window.localStorage.getItem(storageName);
            if (existing) {
                idList = JSON.parse(existing);
                idList = idList.slice(0, 7);
            }
            //add this to top
            idList.unshift(currentStateId);
            //save
            window.localStorage.setItem(storageName, JSON.stringify(idList));
            //update state id selection
            updateStateDropdown();
            //clear chat
            await HandleMessage();
        });

        $('#conv-recent-dropdown').on('change', async function () {
            currentState = this.value;
            //clear chat
            await HandleMessage();
        });

    }
    catch (err) {
        HandleError(err);
    }


}

async function LoadVirtualGraph() {
    try {
        var virtualdata = await virtualkgraphdata({ model: mdname });
        virtualcy = cytoscape({
            container: $('#virtualgraph'),
            elements: virtualdata.getVirtualKGDisplay,
            style: [ // the stylesheet for the graph
                {
                    selector: 'node',
                    style: {
                        'background-color': '#11479e',
                        'label': 'data(lineage)'
                    }
                },
                {
                    selector: 'node:selected',
                    style: {
                        'background-color': '#1010ff',
                        'label': 'data(externalId)'
                    }
                },
                {
                    selector: 'edge',
                    style: {
                        'width': 3,
                        'line-color': '#ccc',
                        'target-arrow-color': '#ccc',
                        'target-arrow-shape': 'triangle',
                        'curve-style': 'bezier',
                        'label': 'data(label)'
                    }
                }
            ],

            layout: {
                name: 'dagre'
            }
        });
        virtualcy.cxtmenu({
            selector: 'node',
            commands: [
                {
                    content: '<span class="fa fa-user fa-2x"></span>',
                    select: async function (ele) {
                        console.log('name');
                        var id = ele.data('lineage');
                        var obj = await virtualobjectdata({ model: mdname, lineage: id });
                        if (obj) {
                            $.MessageBox({
                                input: {
                                    name: {
                                        type: "caption",
                                        message: obj.getVirtualObjectByLineage.name
                                    }
                                },
                                message: "The name",
                                buttonDone: "OK",
                            }).done(function (data) {
                                console.log(data);
                            });
                        }
                    }
                },
                {
                    content: '<span class="fa fa-info fa-2x"></span>',
                    select: async function (ele) {
                        ShowInfo("md/thinkbase/virtual_node.md");
                    }
                },
                {
                    content: '<span class="fa fa-tasks fa-2x"></span>',
                    select: async function (ele) {
                        console.log('attributes');
                        var id = ele.data('lineage');
                        await EditVirtualAttributes(id);
                    }
                },
                {
                    content: '<span class="fa fa-tree fa-2x"></span>',
                    select: async function (ele) {
                        console.log('lineage');
                        try {
                            var id = ele.data('lineage');
                            var obj = await virtualobjectdata({ model: mdname, lineage: id });
                            if (obj) {
                                var lin = obj.getVirtualObjectByLineage.lineage;
                                var typeword = await gettypeword({ lin: lin });
                                $.MessageBox({
                                    input: {
                                        lineage:
                                        {
                                            type: "caption",
                                            message: lin
                                        },
                                        typeword:
                                        {
                                            type: "caption",
                                            message: typeword.getTypeWordForLineage
                                        }
                                    },
                                    message: "The lineage"
                                }).done(function (data) {
                                    console.log(data);
                                });
                            }
                        }
                        catch (err) {
                            HandleError(err);
                        }
                    }
                }
            ]
        });
        virtualcy.on('add', function (event) {
            node = event.target;
            realcy.remove(node);
        });
        virtualcy.on('tap', function (event) {
            node = event.target;
        });
    }
    catch (err) {
        HandleError(err);
    }
}


var charCodeZero = "0".charCodeAt(0);
var charCodeNine = "9".charCodeAt(0);

function isDigitCode(n) {
    return (n.charCodeAt(0) >= charCodeZero && n.charCodeAt(0) <= charCodeNine);
}

function uuidv4() {
    return ([1e7] + -1e3 + -4e3 + -8e3 + -1e11).replace(/[018]/g, c =>
        (c ^ crypto.getRandomValues(new Uint8Array(1))[0] & 15 >> c / 4).toString(16)
    );
}

function ShowInfo(url) {
    $.get(url, function (data) {
        var converter = new showdown.Converter();
        var html = converter.makeHtml(data);
        var div = $("<div>", {
            css: {
                "width": "100%",
                "margin-top": "1rem"
            }
        }).html(html);

        $.MessageBox({
            message: "Information",
            input: div
        }).done(function (data) {
            console.log(data);
        });
    });
}

function HandleError(err) {
    if (Array.isArray(err)) {
        alert(err[0].message);
    }
    else {
        alert(err);
    }
}

async function CreateNewAttribute(id, type) {
    var attlins = await getlineagesinkgatts({ name: mdname });
    var obj = {};
    $.each(attlins.getLineagesInKG, function (i, item) {
        obj[item.lineage] = item.typeWord;
    });
    //new Attribute messageBox
    //set lineage, type
    $.MessageBox({
        input: {
            name: {
                type: "text",
                label: "Attribute name"
            },
            typeSelect:
            {
                type: "select",
                label: "type of the attribute",
                options: {
                    "NUMERIC": "numeric",
                    "CATEGORICAL": "categorical",
                    "TEXTUAL": "textual",
                    "TEMPORAL": "temporal",
                    "DURATION": "duration",
                    "MARKDOWN": "markdown",
                    "RULESET": "ruleset"
                }
            },
            sep_caption: {
                type: "caption",
                message: "select an existing lineage <br/> or enter a new one."
            },
            existinglineage: {
                label: "existing lineages",
                type: "select",
                options: obj
            },
            newlineage: {
                type: "text",
                label: "new lineage"
            }
        },
        message: "Create a new attribute",
        buttonDone: "Create",
        buttonFail: "Cancel",
        filterDone: function (data) {
            if (data.name === "") return "Please supply a name";
            if (data.typeSelect === "") return "Please select a type";
            if (data.newLineage === "" && data.existinglineage === "") return "Choose an existing lineage or add a new word or lineage.";
        }
    }).done(async function (data) {
        //can only set value when creating, not existence
        var newAtt = { type: data.typeSelect, name: data.name, value: "", lineage: "" };
        if (data.existinglineage) {
            newAtt.lineage = data.existinglineage;
            await UpdateAttributeValue(id, newAtt, type);
        }
        else {
            newAtt.lineage = data.newlineage;
            //check if valid lineage
            var ivl = await isvalidlineage({ lin: newAtt.lineage });
            if (!ivl.isValidLineage) {
                var lineages = await getlineagesforword({ word: newAtt.lineage });
                var obj = {};
                $.each(lineages.getLineagesForWord, function (i, item) {
                    obj[item.lineage] = item.typeWord + ": " + item.description;
                });
                $.MessageBox({
                    input: {
                        lin: {
                            label: "Possible lineages",
                            type: "select",
                            options: obj
                        }
                    },
                    buttonDone: "Select",
                    buttonFail: "Cancel",
                    message: "Choose a lineage for this word.",
                    filterDone: function (data) {
                        if (data.lin === "") return "Select a lineage or cancel.";
                    }
                }).done(async function (data) {
                    newAtt.lineage = data.lin;
                    await UpdateAttributeValue(id, newAtt, type);
                });

            }
            else {
                await UpdateAttributeValue(id, newAtt);
            }
        }
    });
}

async function EditRealAttributes(id) {
    try {
        var obj = await realobjectdata({ model: mdname, id: id });
        if (obj) {
            if (obj.getGraphObjectById.properties) {
                var att = {};
                var types = {};
                var values = {};
                $.each(obj.getGraphObjectById.properties, function (i, item) {
                    att[item.lineage] = item.name;
                    types[item.lineage] = item.type;
                    values[item.lineage] = item.value;
                });
                //select existing or add message box
                //make list of properties by name
                $.MessageBox({
                    input:
                    {
                        attChoice: {
                            type: "select",
                            label: "existing attributes",
                            options: att

                        }
                    },
                    buttonDone: {
                        add: "Add",
                        existing: "Edit",
                        delete: "Delete"
                    },
                    buttonFail: "Cancel",
                    message: "Edit or add an attribute",
                    filterDone: function (data, button) {
                        if (data.attChoice === "" && button === "existing") return "Select an attribute to edit";
                        if (data.attChoice === "" && button === "delete") return "Select an attribute to delete";
                    }
                }).done(async function (data, button) {
                    if (button === "add") {
                        await CreateNewAttribute(id, "real");
                    }
                    else if (button === "delete") {
                        if (data.attChoice) {
                            try {
                                await deleterealattribute({ name: mdname, id: id, attLin: data.attChoice });
                            }
                            catch (err) {
                                HandleError(err);
                            }
                        }
                    }
                    else {
                        var newAtt = { value: values[data.attChoice], lineage: data.attChoice, type: types[data.attChoice] };
                        await UpdateAttributeValue(id, newAtt, "real");
                    }
                });
            }
            else {
                await CreateNewAttribute(id, "real")
            }
        }
    }
    catch (err) {
        HandleError(err);
    }
}

async function EditRecognitionAttributes(id) {
    try {
        var obj = await recognitionobjectdata({ model: mdname, id: id });
        if (obj) {
            if (obj.getRecognitionObjectById.properties) {
                const rule = obj.getRecognitionObjectById.properties.find(e => e.lineage === recognizedLineage);
                if (rule) {
                    rule.type = "RULESET";
                    await UpdateAttributeValue(id, rule, "recognition");
                }
                else {
                    const newAtt = { value: "", lineage: recognizedLineage, type: "RULESET" };
                    await UpdateAttributeValue(id, newAtt, "recognition");
                }
            }
            else {
                const newAtt = { value: "", lineage: recognizedLineage, type: "RULESET" };
                await UpdateAttributeValue(id, newAtt, "recognition");
            }
        }
    }
    catch (err) {
        HandleError(err);
    }
}

async function EditVirtualAttributes(id) {
    try {
        var obj = await virtualobjectdata({ model: mdname, lineage: id });
        if (obj) {
            if (obj.getVirtualObjectByLineage.properties) {
                var att = {};
                var types = {};
                var values = {};
                $.each(obj.getVirtualObjectByLineage.properties, function (i, item) {
                    att[item.lineage] = item.name;
                    types[item.lineage] = item.type;
                    values[item.lineage] = item.value;
                });
                //select existing or add message box
                //make list of properties by name
                $.MessageBox({
                    input:
                    {
                        attChoice: {
                            type: "select",
                            label: "existing attributes",
                            options: att

                        }
                    },
                    buttonDone: {
                        add: "Add",
                        existing: "Edit",
                        delete: "Delete"
                    },
                    buttonFail: "Cancel",
                    message: "Edit or add an attribute",
                    filterDone: function (data, button) {
                        if (data.attChoice === "" && button === "existing") return "Select an attribute to edit";
                    }
                }).done(async function (data, button) {
                    if (button === "add") {
                        await CreateNewAttribute(id, "virtual");
                    }
                    else if (button === "delete") {
                        try {
                            await deletevirtualattribute({ name: mdname, lineage: id, attLin: data.attChoice });
                        }
                        catch (err) {
                            HandleError(err);
                        }
                    }
                    else {
                        var newAtt = { value: values[data.attChoice], lineage: data.attChoice, type: types[data.attChoice] };
                        await UpdateAttributeValue(id, newAtt, "virtual");
                    }
                });
            }
            else {
                await CreateNewAttribute(id, "virtual")
            }
        }
    }
    catch (err) {
        HandleError(err);
    }
}

async function CreateNode(evt, name, externalId, lin, sublin) {
    try {
        var created = await createrealobject({ name: mdname, obj: { name: name, externalId: externalId, lineage: lin, subLineage: sublin } });
        realcy.add({
            group: 'nodes',
            data: { label: name, externalId: externalId, id: created.createGraphObject.id, lineage: lin, sublineage: sublin },
            position: {
                x: evt.position.x,
                y: evt.position.y
            }
        });
        await LoadVirtualGraph();

    }
    catch (err) {
        HandleError(err);
    }
}

async function CreateRecognitionNode(evt, lin) {
    try {
        var created = await createrecognitionobject({ name: mdname, obj: { lineage: lin, name: lin } });
        recognitioncy.add({
            group: 'nodes',
            data: { label: lin, id: created.createRecognitionObject.id, lineage: lin },
            position: {
                x: evt.position.x,
                y: evt.position.y
            }
        });
    }
    catch (err) {
        HandleError(err);
    }
}

async function CreateConnection(addedEles, sourceNode, targetNode, name, lin) {
    try {
        var startId = sourceNode.id();
        var endId = targetNode.id();
        var created = await createrealconnection({ name: mdname, conn: { name: name, lineage: lin, startId: startId, endId: endId, id: addedEles.id() } });
        addedEles.data('id', created.createGraphConnection.id);
    }
    catch (err) {
        HandleError(err);
        realcy.remove(addedEles);
    }
}

//get a value for the attribute and save it
async function UpdateAttributeValue(id, newAtt, type) {
    switch (newAtt.type) {
        case "NUMERIC":
            $.MessageBox({
                input: {
                    val: {
                        type: "number",
                        label: "Attribute value",
                        defaultValue: newAtt.value
                    }
                },
                message: "Set the attribute's value",
                buttonDone: "Change",
                buttonFail: "Cancel",
                filterDone: function (data) {
                    if (data.val === "") return "Give a value.";
                }
            }).done(function (valdata) {
                if (newAtt.value !== valdata.val) {
                    newAtt.value = valdata.val;
                    Upsert(id, newAtt, type);
                }
            });
            break;
        case "CATEGORICAL":
        case "TEXTUAL":
            $.MessageBox({
                input: {
                    val: {
                        type: "text",
                        label: "Attribute value",
                        defaultValue: newAtt.value
                    }
                },
                message: "Set the attribute's value",
                buttonDone: "Change",
                buttonFail: "Cancel",
                filterDone: function (data) {
                    if (data.val === "") return "Give a value.";
                }
            }).done(function (valdata) {
                if (newAtt.value !== valdata.val) {
                    newAtt.value = valdata.val;
                    Upsert(id, newAtt, type);
                }
            });
            break;
        case "TEMPORAL":
            $.MessageBox({
                input: {
                    date1: {
                        type: "date",
                        label: "Date 1",
                        defaultValue: newAtt.value
                    },
                    time1: {
                        type: "time",
                        label: "Time 1"
                    }
                },
                message: "Set the attribute's time value",
                buttonDone: "Change",
                buttonFail: "Cancel",
                filterDone: function (data) {
                    if (data.date1 === "") return "Give a value.";
                }
            }).done(function (valData) {
                newAtt.value = valdata.date1 + valdate.time1;
                Upsert(id, newAtt, type);
            });
            break;
        case "MARKDOWN":
             $.MessageBox({
                input: {
                    val: {
                        type: "markdown",
                        defaultValue: newAtt.value,
                        resize: true
                    }
                },
                message: "Set the attribute's value",
                buttonDone: "Change",
                buttonFail: "Cancel",
                filterDone: function (data) {
                    if (data.val === "") return "Give a value.";
                }
            }).done(function (valdata) {
                if (newAtt.value !== valdata[0]) {
                    newAtt.value = valdata[0];
                    Upsert(id, newAtt, type);
                }
            });
            break;
        case "RULESET":
            var def;
            try {
                var alt = await defaultRule({ lineage: newAtt.lineage });
                def = newAtt.value !== "" ? newAtt.value : alt.getSuggestedRuleset;
            }
            catch (err) {
                HandleError(err);
                return;
            }
            $.MessageBox({
                input: {
                    val: {
                        type: "ruleset",
                        defaultValue: def
                    }
                },
                message: "Set the attribute's DARL code",
                buttonDone: "Change",
                buttonFail: "Cancel",
                filterDone: function (data) {
                    if (data.val === "") return "Give a value.";
                }
            }).done(function (valdata) {
                if (newAtt.value !== valdata[0]) {
                    newAtt.value = valdata[0];
                    Upsert(id, newAtt, type);
                }
            });
    }
}

async function Upsert(id, newAtt, type) {
    try {
        if (type === "real")
            await updaterealattribute({ name: mdname, id: id, att: newAtt });
        else if (type === "recognition")
            await updaterecognitionattribute({ name: mdname, id: id, att: newAtt });
        else if (type === "virtual")
            await updatevirtualattribute({ name: mdname, lineage: id, att: newAtt });
    }
    catch (err) {
        HandleError(err);
    }
}

async function check_syntax(code, result_cb) {
    try {
        var result = await lintCall({ darl: code });
        result_cb(result.lintDarlMeta);
    }
    catch (err) {
        HandleError(err);
    }
}

function convertescapes(text) {
    text = text.replace(/&quot;/g, '"').replace(/&#39;/g, "'").replace(/&gt;/g, '>').replace(/&lt;/g, '<').replace(/&#x221E;/g, '∞').replace(/&#xD;/g, '\r').replace(/&#xA;/g, '\n').replace(/&#x9;/g, '\t').replace(/&#x2B;/g, '+').replace(/&#x27;/g, '\'');
    return unescape(text);
}

async function HandleMessage() {
    var tags = [
            {
            type: "input",
            tag: "text",
            name: "response",
            "chat-msg": "Type below to test/interrogate " + mdname + " with the selected knowledge state",
            success: async function (data) {
                await MessageProcess(data);
            }
        }
    ];

    Chat.start($('#chat'), tags);

}

async function MessageProcess(data) {
    try {
        var res = await interact({ name: mdname, ksid: currentStateId, text: Array.isArray(data.response) ? data.response[0] : data.response });
        for (let i = 0, n = res.interactKnowledgeGraph.length; i < n; i++){
            let r = res.interactKnowledgeGraph[i];
            //only wait on the last
            if (i === n - 1) {
                switch (r.response.dataType) {
                    case "textual":
                    case "numeric":
                        Chat.addTags(
                            [{
                                type: "input",
                                tag: "text",
                                "chat-msg": r.response.value,
                                name: "response",
                                success: async function (data) {
                                    await MessageProcess(data);
                                }
                            }]
                        );
                        break;
                    case "categorical":
                        var cats = [];
                        for (let n of r.response.categories) {
                            cats.push({ value: n.name, text: n.name });
                        }
                        Chat.addTags(
                            [{
                                type: "input",
                                tag: "radio",
                                name: "response",
                                "chat-msg": r.response.value,
                                children: cats,
                                success: async function (data) {
                                    await MessageProcess(data);
                                }
                            }]);

                        break;
                }
            }
            else {
                Chat.addTextResponse(r.response.value);
/*                Chat.addTags(
                        [{
                            type: "msg",
                            tag: "text",
                            "chat-msg": r.response.value,
                            name: "response"
                         }]
                    );*/
            }
            
        }
        UpdateKS();
    }
    catch (err) {
        HandleError(err);
    }
}

async function UpdateKS() {
    try {
        const resp = await getks({ id: currentStateId });
        $('#kstate').jsonViewer(resp.getKnowledgeState, { collapsed: true, withQuotes: false, withLinks: true });
    }
    catch (err) {
        HandleError(err);
    }
}
