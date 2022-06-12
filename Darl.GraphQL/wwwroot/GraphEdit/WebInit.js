
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
var updatekg;
var kgraph;
var edited = false;
var recognizedLineage = "adjective:8953";
var textLineage = "noun:01,4,04,02,07,01";
var completedLineage = "adjective:5500";
var currentStateId;
var settingsStorageName = 'thinkbase-settings';
var realStorageName = 'thinkbase-real';
var virtualStorageName = 'thinkbase-virtual';
var recognitionStorageName = 'thinkbase-recognition';
var demo = false;
var dateDisplay = "Recent";
var authoritative = true;
var labels = "externalId";
var inferenceTime = "Now";
var descriptions;
var initialTexts;
var dateDisplays;
var inferenceTimes;
var fixedTimes;
var virtualLabels = "label";
var recLabels = "label";
var lastConnectionName = "";
var lastConnectionExistingLineage = "";
var charCodeZero = "0".charCodeAt(0);
var charCodeNine = "9".charCodeAt(0);


$(async function () {

    window.addEventListener('beforeunload', (event) => {
        if (edited) {
            event.returnValue = 'You have unfinished changes!';
        }
    });
    var existing = window.localStorage.getItem(settingsStorageName);
    existing = JSON.parse(existing);
    var url = window.location.origin;
    var key = "";
    if (existing) {
        if (existing.url) {
            url = existing.url;
            if (url === "https://localhost:44311/index")
                url = "https://localhost:44311";
        }
        if (existing.key) {
            key = existing.key;
        }
    }
    // Get real window settings
    var real = window.localStorage.getItem(realStorageName);
    authoritative = (real ? (real.authoritative ? real.authoritative : true) : true);
    labels = (real ? (real.labels ? real.labels : "externalId") : "externalId");

    var virtual = window.localStorage.getItem(virtualStorageName);
    virtualLabels = (virtual ? (virtual.virtualLabels ? virtual.virtualLabels : "label") : "label");

    var recset = window.localStorage.getItem(virtualStorageName);
    recLabels = (recset ? (recset.recLabels ? recset.recLabels : "label") : "label");;


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
        $('#fileHandling').addClass('d-none');
        if (!mdname) {
            $('#demohandling').removeClass('d-none');
        }
    }

    allkgmodels = graph(`{ kgraphs { name model {description initialText dateDisplay inferenceTime fixedTime{raw dateTimeOffset dateTime}}}}`);
    kgraph = graph('query kg($name: String!){kGraphByName(name: $name){name model {description initialText dateDisplay inferenceTime fixedTime{raw dateTimeOffset dateTime}}}}');
    realkgraphdata = graph('query kgd($model: String!){getRealKGDisplay(graphName: $model){nodes{data{ id label lineage sublineage externalId}} edges{ data{ id label source target}}}}');
    virtualkgraphdata = graph('query vkgd($model: String!){getVirtualKGDisplay(graphName: $model){nodes{data{ id lineage parent label}} edges{ data{ id label source target}}}}');
    recognitionkgraphdata = graph('query rkgd($model: String!){getRecognitionKGDisplay(graphName: $model){nodes{data{ id label lineage parent label}} edges{ data{ id label source target}}}}');
    realobjectdata = graph('query rod($model: String! $id: String!){getGraphObjectById(graphName: $model id: $id){name lineage subLineage id externalId properties {name lineage value type confidence properties {name lineage value type confidence}}, existence{dateTimeOffset dateTime}}}');
    realConnectiondata = graph('query rcd($model: String! $id: String!){getGraphConnectionById(graphName: $model id: $id){name lineage id weight inferred }}');
    virtualobjectdata = graph('query vod($model: String! $lineage: String!){getVirtualObjectByLineage(graphName: $model lineage: $lineage){name lineage id properties {name lineage value type confidence}}}');
    recognitionobjectdata = graph('query recod($model: String! $id: String!){getRecognitionObjectById(graphName: $model id: $id){name lineage id properties {name lineage value type confidence}}}');
    realeditorchange = graph('mutation rec($model: String! $goj: String!){updateGraphObjectFromJSON(graphName: $model graphObjectJSON: $goj ontology: BUILD) {id}}');
    virtualeditorchange = graph('mutation uvg($model: String! $goj: String!){updateVirtualGraphObjectFromJSON(graphName: $model graphObjectJSON: $goj) {id}}');
    receditorchange = graph('mutation rec($model: String! $goj: String!){updateRecognitionObjectFromJSON(graphName: $model graphObjectJSON: $goj) {id}}');
    createKG = graph('mutation createKG($name: String!){createKGraph(name: $name)}');
    savechanges = graph('mutation saveKGraph($name: String!){saveKGraph(name: $name)}');
    deleterealobject = graph('mutation dgo($name: String! $id: String!){deleteGraphObject(graphName: $name id: $id){name}}');
    deleterealconnection = graph('mutation dgc($name: String! $id: String!){deleteGraphConnection(graphName: $name id: $id){name}}');
    deleterecognitionobject = graph('mutation dgc($name: String! $id: String!){deleteRecognitionObject(name: $name id: $id)}');
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
    updaterealattribute = graph('mutation uga($name: String! $id: String! $att: graphAttributeInput!){updateGraphObjectAttribute(name: $name id: $id att: $att){value}}');
    updaterecognitionattribute = graph('mutation uga($name: String! $id: String! $att: graphAttributeInput!){updateRecognitionObjectAttribute(name: $name id: $id att: $att)}');
    updatevirtualattribute = graph('mutation uga($name: String! $lineage: String! $att: graphAttributeInput!){updateVirtualObjectAttribute(name: $name lineage: $lineage att: $att)}');
    createrecognitionroot = graph('mutation crr($name: String! $lineage: String!){createRecognitionRoot(name: $name lineage: $lineage ){id}}');
    updaterecognitionobject = graph('mutation uro($name: String! $obj: graphObjectUpdate!){updateRecognitionObject(name: $name object: $obj){id}}');
    lintCall = graph('query lint($darl: String!){  lintDarlMeta(darl: $darl){ column_no_start column_no_stop line_no message severity }}');
    interact = graph('query int($name: String! $ksid: String! $text:  String!){interactKnowledgeGraph(kgModelName: $name conversationId: $ksid conversationData: { dataType: textual name: "" value: $text }){ darl reference response{dataType name value categories{name value }}}}');
    defaultRule = graph('query dr($name: String! $id: String! $lineage: String!){getSuggestedRuleset(graphName: $name objectId: $id lineage: $lineage)}');
    getks = graph('query gks($id: String!){getInteractKnowledgeState(id: $id external: true){knowledgeGraphName data {name value {name lineage value confidence type }}}}')
    deletekg = graph('mutation dkg($name: String!){deleteKG(name: $name)}');
    updatekg = graph('mutation ukg($name: String! $update: modelMetaDataUpdate!){updateKGraphMetadata(name: $name update: $update){description dateDisplay inferenceTime fixedTime{raw dateTimeOffset dateTime precision}}}')

    if (mdname !== null) { // pre-loaded
        $('#fileHandling').addClass('d-none');
        descriptions = {};
        initialTexts = {};
        dateDisplays = {};
        inferenceTimes = {};
        fixedTimes = {};
        try {
            var kgmeta = await kgraph({ name: mdname });
            descriptions[mdname] = kgmeta.kGraphByName.model.description;
            initialTexts[mdname] = kgmeta.kGraphByName.model.initialText;
            dateDisplays[mdname] = kgmeta.kGraphByName.model.dateDisplay;
            inferenceTimes[mdname] = kgmeta.kGraphByName.model.inferenceTime;
            fixedTimes[mdname] = kgmeta.kGraphByName.model.fixedTime;
        }
        catch (err) {
            window.location.replace("/index");
            return;
        }
        $('#graphTitle').text(mdname);
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
            if (res.createKGraph == false) {
                alert("Couldn't create " + mdname + ". Do you need to uprade your account?");
            }
            else {
                await updateDropdown();
                alert(mdname + " created");
            }
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
            queue: false,
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
            }
        });
    });

    $('#kg-delete').click(async function () {
        if (!mdname) {
            $.MessageBox("No KG is selected.");
            return;
        }
        $.MessageBox({
            buttonDone: "Yes",
            buttonFail: "No",
            queue: false,
            message: "Delete this Knowledge Graph?"
        }).done(async function () {
            try {
                await deletekg({ name: mdname });
                realcy.destroy();
                virtualcy.destroy();
                recognitioncy.destroy();
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
            edited = false;
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
            queue: false,
            filterDone: function (data) {
                if (data.url === "") return "You must supply a url.";
            }
        }).done(async function (data) {
            window.localStorage.setItem(settingsStorageName, JSON.stringify(data));
            //update 
        });
    });

});
