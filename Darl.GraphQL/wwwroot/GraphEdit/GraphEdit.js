
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
    try {
        const rs = await allkgmodels();
        var dropdown = demo ? $('#kgdemo-dropdown') : $('#kgmodel-dropdown');
        dropdown.empty();
        if (demo) {
            dropdown.append('<option selected="true" disabled>Choose a Knowledge Graph to view</option>');
        }
        else {
            dropdown.append('<option selected="true" disabled>Choose a Knowledge Graph to edit</option>');
        }
        dropdown.prop('selectedIndex', 0);
        descriptions = {};
        initialTexts = {};
        dateDisplays = {};
        inferenceTimes = {};
        fixedTimes = {};
        $.each(rs.kgraphs, function (key, entry) {
            dropdown.append($('<option class="dropdown-item"></option>').attr('value', entry.name).text(entry.name));
            descriptions[entry.name] = entry.model.description;
            initialTexts[entry.name] = entry.model.initialText;
            dateDisplays[entry.name] = entry.vdateDisplay;
            inferenceTimes[entry.name] = entry.model.inferenceTime;
            fixedTimes[entry.name] = entry.model.fixedTime;
        });
    }
    catch (err) {
        HandleError(err);
        return;
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
        if (descriptions[mdname]) {
            var converter = new showdown.Converter();
            var html = converter.makeHtml(descriptions[mdname]);
            var div = $("<div>", {
                css: {
                    "width": "100%",
                    "margin-top": "1rem"
                }
            }).html(html);

            $.MessageBox({
                message: "About this Knowledge Graph",
                input: div,
                queue: false
            });
        }
        if (initialTexts[mdname]) {
            $('#msg_input').val(initialTexts[mdname])
        }
        var realdata = await realkgraphdata({ model: mdname });
        //instantiate graphs here
        realcy = cytoscape({
            container: $('#realgraph'),
            elements: realdata.getRealKGDisplay,
            style: [ // the stylesheet for the graph
                {
                    selector: 'node',
                    style: {
                        'background-color': '#11479e',
                        'label': 'data(' + labels + ')'
                    }
                },
                {
                    selector: 'node:selected',
                    style: {
                        'background-color': '#1010ff',
                        'label': 'data(' + labels + ')'
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
                            queue: false,
                            message: "Are you sure you want to delete this node?"
                        }).done(async function (data) {
                            console.log(data);
                            try {
                                await deleterealobject({ name: mdname, id: ele.id() });
                                realcy.remove(ele);
                                edited = true;
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
                        ShowInfo("/md/thinkbase/real_node.md");
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
                        if (obj)
                        {
                            if (dateDisplays[mdname] === "Historic") {
                                $.MessageBox({
                                    input: {
                                        date1: {
                                            type: "number",
                                            label: "Year 1, -ve for BC"
                                        },
                                        time1: {
                                            type: "select",
                                            label: "Season 1",
                                            options: ["Winter", "Spring", "Summer", "Fall"]
                                        },
                                        date2: {
                                            type: "number",
                                            label: "Year 2, -ve for BC"
                                        },
                                        time2: {
                                            type: "select",
                                            label: "Season 2",
                                            options: ["Winter", "Spring", "Summer", "Fall"]
                                        },
                                        date3: {
                                            type: "number",
                                            label: "Year 3, -ve for BC"
                                        },
                                        time3: {
                                            type: "select",
                                            label: "Season 3",
                                            options: ["Winter", "Spring", "Summer", "Fall"]
                                        },
                                        date4: {
                                            type: "number",
                                            label: "Year 4, -ve for BC"
                                        },
                                        time4: {
                                            type: "select",
                                            label: "Season 4",
                                            options: ["Winter", "Spring", "Summer", "Fall"]
                                        },
                                        dummy_caption: {
                                            type: "caption",
                                            message: "One value constitutes an event, two an interval, <br/>three a fuzzy event and four a fuzzy interval.<br/>Entries will be sorted in time-order before use."
                                        }
                                    },
                                    message: "Set or change the times of this element's existence",
                                    buttonDone: "Change",
                                    buttonFail: "Cancel",
                                    queue: false
                                }).done(async function (data) {
                                    var times = new Array();
                                    if (data.date1) {
                                        if (data.time1) {
                                            times.push({ dateTimeOffset: data.date1 + "T" + data.time1 });
                                        }
                                        else {
                                            times.push({ dateTimeOffset: data.date1 });
                                        }
                                    }
                                    if (data.date2) {
                                        if (data.time2) {
                                            times.push({ dateTimeOffset: data.date2 + "T" + data.time2 });
                                        }
                                        else {
                                            times.push({ dateTimeOffset: data.date2 });
                                        }
                                    }
                                    if (data.date3) {
                                        if (data.time3) {
                                            times.push({ dateTimeOffset: data.date3 + "T" + data.time3 });
                                        }
                                        else {
                                            times.push({ dateTimeOffset: data.date3 });
                                        }
                                    }
                                    if (data.date4) {
                                        if (data.time4) {
                                            times.push({ dateTimeOffset: data.date4 + "T" + data.time4 });
                                        }
                                        else {
                                            times.push({ dateTimeOffset: data.date4 });
                                        }
                                    }
                                    try {
                                        await updateGraphObject({ name: mdname, obj: { id: ele.id(), existence: times, lineage: ele.data('lineage') } });
                                    }
                                    catch (err) {
                                        HandleError(err);
                                    }
                                    console.log(data);
                                });
                            }
                            else {
                                $.MessageBox({
                                    input: {
                                        date1: {
                                            type: "date",
                                            label: "Date 1",
                                            defaultValue: HandleDates(obj.getGraphObjectById.existence, 0)
                                        },
                                        time1: {
                                            type: "time",
                                            label: "Time 1",
                                            defaultValue: HandleTimes(obj.getGraphObjectById.existence, 0)
                                        },
                                        date2: {
                                            type: "date",
                                            label: "Date 2",
                                            defaultValue: HandleDates(obj.getGraphObjectById.existence, 1)
                                        },
                                        time2: {
                                            type: "time",
                                            label: "Time 2",
                                            defaultValue: HandleTimes(obj.getGraphObjectById.existence, 1)
                                        },
                                        date3: {
                                            type: "date",
                                            label: "Date 3",
                                            defaultValue: HandleDates(obj.getGraphObjectById.existence, 2)
                                        },
                                        time3: {
                                            type: "time",
                                            label: "Time 3",
                                            defaultValue: HandleTimes(obj.getGraphObjectById.existence, 2)
                                        },
                                        date4: {
                                            type: "date",
                                            label: "Date 4",
                                            defaultValue: HandleDates(obj.getGraphObjectById.existence, 3)
                                        },
                                        time4: {
                                            type: "time",
                                            label: "Time 4",
                                            defaultValue: HandleTimes(obj.getGraphObjectById.existence, 3)
                                        },
                                        dummy_caption: {
                                            type: "caption",
                                            message: "One value constitutes an event, two an interval, <br/>three a fuzzy event and four a fuzzy interval.<br/>Entries will be sorted in time-order before use."
                                        }
                                    },
                                    message: "Set or change the times of this element's existence",
                                    queue: false,
                                    buttonDone: "Change",
                                    buttonFail: "Cancel"
                                }).done(async function (data) {
                                    var times = new Array();
                                    if (data.date1) {
                                        if (data.time1) {
                                            times.push({ dateTimeOffset: data.date1 + "T" + data.time1 });
                                        }
                                        else {
                                            times.push({ dateTimeOffset: data.date1 });
                                        }
                                    }
                                    if (data.date2) {
                                        if (data.time2) {
                                            times.push({ dateTimeOffset: data.date2 + "T" + data.time2 });
                                        }
                                        else {
                                            times.push({ dateTimeOffset: data.date2 });
                                        }
                                    }
                                    if (data.date3) {
                                        if (data.time3) {
                                            times.push({ dateTimeOffset: data.date3 + "T" + data.time3 });
                                        }
                                        else {
                                            times.push({ dateTimeOffset: data.date3 });
                                        }
                                    }
                                    if (data.date4) {
                                        if (data.time4) {
                                            times.push({ dateTimeOffset: data.date4 + "T" + data.time4 });
                                        }
                                        else {
                                            times.push({ dateTimeOffset: data.date4 });
                                        }
                                    }
                                    try {
                                        await updateGraphObject({ name: mdname, obj: { id: ele.id(), existence: times, lineage: ele.data('lineage') } });
                                    }
                                    catch (err) {
                                        HandleError(err);
                                    }
                                    console.log(data);
                                });
                            }
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
                                    queue: false,
                                    filterDone: function (data) {
                                        if (data === "") return "ExternalId must have a value!";
                                    }
                                }).done(async function (data) {
                                    if (obj.getGraphObjectById.externalId !== data) {
                                        try {
                                            await updateGraphObject({ name: mdname, obj: { id: ele.id(), externalId: data, lineage: ele.data('lineage') } });
                                            ele.data('externalId', data);
                                        }
                                        catch (err) {
                                            HandleError(err);
                                        }
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
                                    queue: false,
                                    filterDone: function (data) {
                                        if (data === "") return "Name must have a value!";
                                    }
                                }).done(async function (data) {
                                    if (obj.getGraphObjectById.name !== data) {
                                        try {
                                            await updateGraphObject({ name: mdname, obj: { id: ele.id(), name: data, lineage: ele.data('lineage') } });
                                            ele.data('label', data);
                                        }
                                        catch (err) {
                                            HandleError(err);
                                        }
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
                                            type: "text",
                                            label: "The sub-lineage",
                                            defaultValue: sublin
                                        },
                                        subtypeword:
                                        {
                                            type: "caption",
                                            message: subTypeword
                                        }
                                    },
                                    message: "The lineage",
                                    queue: false,
                                    buttonDone: "Change",
                                    buttonFail: "Cancel",
                                }).done(async function (data) {
                                    console.log(data);
                                    if (data.sublineage !== sublin) {
                                        const ivl = await isvalidlineage({ lin: data.sublineage });
                                        if (!ivl.isValidLineage) {
                                            var sublineages = await getlineagesforword({ word: data.sublineage });
                                            var subobj = {};
                                            $.each(sublineages.getLineagesForWord, function (i, item) {
                                                subobj[item.lineage] = item.typeWord + ": " + item.description;
                                            });
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
                                                queue: false,
                                                filterDone: function (data) {
                                                    if (data.lin === "") return "Select a lineage.";
                                                }
                                            }).done(async function (data) {
                                                sublin = data.lin;
                                                try {
                                                    await updateGraphObject({ name: mdname, obj: { id: ele.id(), subLineage: sublin, lineage: lin } });
                                                }
                                                catch (err) {
                                                    HandleError(err);
                                                }
                                            });
                                        }
                                        else {
                                            try {
                                                await updateGraphObject({ name: mdname, obj: { id: ele.id(), subLineage: data.sublineage, lineage: lin } });
                                            }
                                            catch (err) {
                                                HandleError(err);
                                            }
                                        }
                                    }
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
                            queue: false,
                            message: "Are you sure you want to delete this node?"
                        }).done(async function (data) {
                            try {
                                console.log(data);
                                await deleterealconnection({ name: mdname, id: ele.id() });
                                realcy.remove(ele);
                                edited = true;
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
                                queue: false,
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
                                try {
                                    await updateGraphConnection({ name: mdname, conn: { id: ele.id(), existence: times } });
                                }
                                catch (err) {
                                    HandleError(err);
                                }
                                console.log(data);
                            });
                        }
                    }
                },
                {
                    content: '<span class="fa fa-info fa-2x"></span>',
                    select: async function (ele) {
                        ShowInfo("/md/thinkbase/real_Connection.md");
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
                                    queue: false,
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
                                queue: false,
                                message: "The lineage"
                            }).done(function (data) {
                                console.log(data);
                            });
                        }
                    }
                },
                {
                    content: '<span class="fa fa-balance-scale fa-2x"></span>',
                    select: async function (ele) {
                        console.log('weight');
                        try {
                            var id = ele.id();
                            var obj = await realConnectiondata({ model: mdname, id: id });
                            if (obj) {
                                $.MessageBox({
                                    input: {
                                        weight: {
                                            type: "number",
                                            defaultValue: obj.getGraphConnectionById.weight,
                                            label: "weight (0 - 1)",
                                            htmlAttributes: {
                                                "min": "0.0",
                                                "max": "1.0",
                                                "step": "0.01"
                                            }
                                        }
                                    },
                                    message: "Edit the weight",
                                    buttonDone: "Change",
                                    buttonFail: "Cancel",
                                    queue: false,
                                    filterDone: function (data) {
                                        if (data.weight === "") return "Weight must have a value";
                                        var weight = parseFloat(data.weight);
                                        if (isNaN(weight))
                                            return "Weight must be a number";
                                        if (weight > 1.0 || weight < 0.0)
                                            return "Weight must be between 0 and 1";
                                    }
                                }).done(async function (data) {
                                    try {
                                        if (obj.getGraphConnectionById.weight !== parseFloat(data.weight)) {
                                            await updateGraphConnection({ name: mdname, conn: { id: ele.id(), weight: data.weight } });
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
                    content: '<span class="fa fa-check-square fa-2x"></span>',
                    select: async function (ele) {
                        console.log('inferred');
                        try {
                            var id = ele.id();
                            var obj = await realConnectiondata({ model: mdname, id: id });
                            if (obj) {
                                $.MessageBox({
                                    input: {
                                        inferred: {
                                            type: "checkbox",
                                            defaultValue: obj.getGraphConnectionById.inferred,
                                            label: "inferred?"
                                        }
                                    },
                                    message: "Can this connection be inferred?",
                                    buttonDone: "Change",
                                    buttonFail: "Cancel",
                                    queue: false,
                                }).done(async function (data) {
                                    try {
                                        if (obj.getGraphConnectionById.inferred !== parseFloat(data.inferred)) {
                                            await updateGraphConnection({ name: mdname, conn: { id: ele.id(), inferred: data.inferred } });
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
                        queue: false,
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
                        if (data.existinglineage) {//no lookup needed
                            lin = data.existinglineage;
                        }
                        if (data.existingsublineage) {
                            sublin = data.existingsublineage;
                        }
                        if (!lin || (!sublin && data.newsublineage !== "")) {
                            //check if new lineage and new sublineage are valid lineages
                            const ivl = await isvalidlineage({ lin: data.newlineage });
                            if (ivl.isValidLineage) {
                                lin = data.newlineage;
                            }
                            if (data.newsublineage !== "") {
                                const ivl2 = await isvalidlineage({ lin: data.newsublineage });
                                if (ivl2.isValidLineage) {
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
                                        queue: false,
                                        message: "Choose primary and sub-lineage for these words.",
                                        filterDone: function (data) {
                                            if (data.lin === "" || data.sublin === "") return "Select lineages.";
                                        }
                                    }).done(async function (data) {
                                        sublin = data.sublin;
                                        lin = data.lin;
                                        try {
                                            await CreateNode(evt, name, externalId, lin, sublin);
                                        }
                                        catch (err) {
                                            HandleError(err);
                                        }
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
                                        queue: false,
                                        filterDone: function (data) {
                                            if (data.lin === "") return "Select a lineage.";
                                        }
                                    }).done(async function (data) {
                                        sublin = data.lin;
                                        try {
                                            await CreateNode(evt, name, externalId, lin, sublin);
                                        }
                                        catch (err) {
                                            HandleError(err);
                                        }
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
                                        queue: false,
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
                                try {
                                    await CreateNode(evt, name, externalId, lin, sublin);
                                }
                                catch (err) {
                                    HandleError(err);
                                }
                            }
                        }
                        else {
                            try {
                                await CreateNode(evt, name, externalId, lin);
                            }
                            catch (err) {
                                HandleError(err);
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
                        label: "Name",
                        defaultValue: lastConnectionName
                    },
                    sep_caption: {
                        type: "caption",
                        message: "select an existing lineage <br/> or enter a new one."
                    },
                    existinglineage: {
                        label: "existing lineages",
                        type: "select",
                        options: obj,
                        defaultValue: lastConnectionExistingLineage
                    },
                    newlineage: {
                        type: "text",
                        label: "new lineage"
                    }
                },
                buttonDone: "Add",
                buttonFail: "Cancel",
                queue: false,
                message: "Add a new edge to the network.",
                filterDone: function (data) {
                    if (data.name === "") return "Please supply a name";
                    if (data.newLineage === "" && data.existinglineage === "") return "Choose an existing lineage or add a new word or lineage.";
                }

            }).done(async function (data) {
                var lin;
                lastConnectionName = data.name;
                if (data.existinglineage) {
                    lin = data.existinglineage;
                    lastConnectionExistingLineage = data.existinglineage;
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
                                queue: false,
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

        await LoadRecGraph();



        realchanged = true;
        virtualchanged = true;
        recchanged = true;

        $('#real-find').click(async function () {
            $.MessageBox({
                input: true,
                message: labels + " to search for:",
                buttonDone: "Find",
                buttonFail: "Cancel",
                queue: false
            }).done(function (data) {
                if ($.trim(data)) {
                    var nodes = realcy.nodes().filter(function (element, i) {
                        return element.data(labels) === $.trim(data);
                    });
                    realcy.fit(nodes, 300);
                    nodes.emit('tap');
                }
            });
        });

        $('#real-help').click(function () {
            ShowInfo("/md/thinkbase/real_view.md");
        });

        $('#real-time').click(function () {
            $.MessageBox({
                input: {
                    dateDisplay: {
                        type: "select",
                        label: "Select how dates are displayed",
                        options: ["Recent", "Historic"],
                        defaultValue: dateDisplays[mdname] === "RECENT" ? "Recent" : "Historic"
                    },
                    inferenceTime:
                    {
                        type: "select",
                        defaultValue: inferenceTimes[mdname] === "FIXED" ? "Fixed" : "Now",
                        options: ["Now","Fixed"]
                    }
                },
                message: "Use the current time or some fixed time to make inferences",
                buttonDone: "Change",
                buttonFail: "Cancel",
                queue: false,
                filterDone: function (data) {
                    if (data.inferenceTime === "" || data.dateDisplay === "") return "Select an inference time and display format or cancel.";
                }
            }).done(async function (data) {
                dateDisplays[mdname] = data.dateDisplay;
                inferenceTimes[mdname] = data.inferenceTime;
                if (inferenceTimes[mdname] === "Fixed") { //get the fixed time
                    if (data.dateDisplay === "Historic") {
                        $.MessageBox({
                            input: {
                                year: {
                                    type: "number",
                                    label: "Year, -ve for BC",
                                    defaultValue: ConvertRawToYear(fixedTimes[mdname].raw)
                                },
                                season: {
                                    type: "select",
                                    label: "season",
                                    options: ["Winter", "Spring", "Summer", "Fall"],
                                    defaultValue: ConvertRawToSeason(fixedTimes[mdname].raw)
                                }
                            },
                            message: "Fixed time for inference",
                            buttonDone: "Change",
                            buttonFail: "Cancel",
                            queue: false
                        }).done(async function (data) {
                            var rawtime = ConvertHistoricDateTime(data.year, data.season);
                            try {
                                fixedTimes[mdname].raw = rawTime;
                                await updatekg({ name: mdname, update: { fixedTime: { raw: rawtime }, inferenceTime: inferenceTimes[mdname] } });
                            }
                            catch (err) {
                                HandleError(err);
                            }
                        });
                    }
                    else { //standard format
                        let timePart;
                        let datePart;
                        if (fixedTimes[mdname] !== null) {
                            let sep = fixedTimes[mdname].dateTimeOffset.indexOf('T');
                            timePart = fixedTimes[mdname].dateTimeOffset.substring(sep + 1, 9 + sep);
                            datePart = fixedTimes[mdname].dateTimeOffset.substring(0, sep);
                        }
                        $.MessageBox({
                            input: {
                                date: {
                                    type: "date",
                                    label: "Fixed Date",
                                    defaultValue: datePart
                                },
                                time: {
                                    type: "time",
                                    label: "Fixed Time",
                                    defaultValue: timePart
                                }
                            },
                            message: "Fixed time for inference",
                            queue: false,
                            buttonDone: "Change",
                            buttonFail: "Cancel",
                        }).done(async function (data) {
                            try {
                                if (data.time) {
                                    fixedTimes[mdname] = { dateTimeOffset: data.date + "T" + data.time };
                                    await updatekg({ name: mdname, update: { fixedTime: { dateTimeOffset: data.date + "T" + data.time }, inferenceTime: inferenceTimes[mdname], dateDisplay: dateDisplays[mdname] } });
                                }
                                else {
                                    fixedTimes[mdname] = { dateTimeOffset: data.date };
                                    await updatekg({ name: mdname, update: { fixedTime: { dateTimeOffset: data.date }, inferenceTime: inferenceTimes[mdname], dateDisplay: dateDisplays[mdname] } });
                                }
                            }
                            catch (err) {
                                HandleError(err);
                            }
                        });
                    }
                }
                else {
                    try {
                        await updatekg({ name: mdname, update: { inferenceTime: inferenceTimes[mdname], dateDisplay: dateDisplays[mdname] } });
                    }
                    catch (err) {
                        HandleError(err);
                    }
                }
            });
        });

        $('#real-settings').click(function () {
            $.MessageBox({
                input: {
                    authoritative: {
                        type: "checkbox",
                        label: "New lineage relationships are authoritative.",
                        defaultValue: authoritative
                    },
                    labels: {
                        type: "select",
                        label: "Select data displayed on nodes",
                        options: ["externalId", "label"],
                        defaultValue: labels
                    },
                },
                message: "Settings",
                queue: false,
                buttonDone: "Save",
                buttonFail: "Cancel",
            }).done(async function (data) {
                window.localStorage.setItem(realStorageName, JSON.stringify(data));
                dateDisplay = data.dateDisplay;
                authoritative = data.authoritative;
                if (labels !== data.labels) {
                    //redraw real graph.
                    labels = data.labels;
                    await loadGraphs();
                }
            });
        });

        $('#virtual-settings').click(function () {
            $.MessageBox({
                input: {

                    virtualLabels: {
                        type: "select",
                        label: "Select data displayed on nodes",
                        options: ["lineage", "label"],
                        defaultValue: virtualLabels
                    },
                },
                message: "Settings",
                queue: false,
                buttonDone: "Save",
                buttonFail: "Cancel",
            }).done(async function (data) {
                window.localStorage.setItem(virtualStorageName, JSON.stringify(data));
                if (virtualLabels !== data.virtualLabels) {
                    //redraw virtual graph.
                    virtualLabels = data.virtualLabels;
                    await LoadVirtualGraph();
                }
            });
        });

        $('#rec-settings').click(function () {
            $.MessageBox({
                input: {

                    recLabels: {
                        type: "select",
                        label: "Select data displayed on nodes",
                        options: ["lineage", "label"],
                        defaultValue: recLabels
                    },
                },
                message: "Settings",
                queue: false,
                buttonDone: "Save",
                buttonFail: "Cancel",
            }).done(async function (data) {
                window.localStorage.setItem(recognitionStorageName, JSON.stringify(data));
                if (recLabels !== data.recLabels) {
                    //redraw recognition graph.
                    recLabels = data.recLabels;
                    await LoadRecGraph();
                }
            });
        });

        $('#real-description').click(async function () {
            if (descriptions[mdname]) {
                var converter = new showdown.Converter();
                var html = converter.makeHtml(descriptions[mdname]);
                var div = $("<div>", {
                    css: {
                        "width": "100%",
                        "margin-top": "1rem"
                    }
                }).html(html);

                $.MessageBox({
                    message: "About this Knowledge Graph",
                    input: div,
                    buttonDone: "Edit",
                    buttonFail: "OK",
                    queue: false
                }).done(async function (data) { await EditKGDescription(); });
            }
           if(!descriptions[mdname]) { // edit the description
               await EditKGDescription();
           }
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
                queue: false,
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
                    edited = true;

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
            ClearChatText();
        });

        $('#conv-recent-dropdown').on('change', async function () {
            currentState = this.value;
            //clear chat
            ClearChatText();
        });

        $('#conv-newstate').click();

        //add message handlers
        $('.msg_send_btn').click(async function () {
            const text = $('.write_msg').val();
            if(text !== "")
                await HandleChatText(text);
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
                        'label': 'data(' + virtualLabels + ')'
                    }
                },
                {
                    selector: 'node:selected',
                    style: {
                        'background-color': '#1010ff',
                        'label': 'data(' + virtualLabels + ')'
                    }
                },
                {
                    selector: 'edge',
                    style: {
                        'width': 3,
                        'line-color': '#9dbaea',
                        'target-arrow-color': '#9dbaea',
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
                                queue: false,
                            }).done(function (data) {
                                console.log(data);
                            });
                        }
                    }
                },
                {
                    content: '<span class="fa fa-info fa-2x"></span>',
                    select: async function (ele) {
                        ShowInfo("/md/thinkbase/virtual_node.md");
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
                                    message: "The lineage",
                                    queue: false,
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

async function LoadRecGraph() {
    try {
        var recdata = await recognitionkgraphdata({ model: mdname });
        recognitioncy = cytoscape({
            container: $('#recognitiongraph'),
            elements: recdata.getRecognitionKGDisplay,
            style: [ // the stylesheet for the graph
                {
                    selector: 'node',
                    style: {
                        'background-color': '#11479e',
                        'label': 'data(' + recLabels + ')'
                    }
                },
                {
                    selector: 'node:selected',
                    style: {
                        'background-color': '#1010ff',
                        'label': 'data(' + recLabels + ')'
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
                        'line-color': '#9dbaea',
                        'target-arrow-color': '#9dbaea',
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
                            queue: false,
                            message: "Are you sure you want to delete this node?"
                        }).done(async function (data) {
                            try {
                                console.log(data);
                                await deleterecognitionobject({ name: mdname, id: ele.id() });
                                realcy.remove(ele);
                                edited = true;
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
                        ShowInfo("/md/thinkbase/recognition_node.md");
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
                    content: '<span class="fa fa-file-alt fa-2x"></span>',
                    select: async function (ele) {
                        if (ele.hasClass("eh-handle")) {
                            ele = ele.data("mainNode");
                        }
                        console.log('text attribute');
                        var id = ele.id();
                        await EditRecognitionMarkDown(id);
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
                                queue: false,
                                filterDone: function (data) {
                                    if (data.word === "") return "Supply a word.";
                                }
                            }).done(async function (data) {
                                try {
                                    console.log('add node ' + data);
                                    var newObj = { id: obj.id, lineage: data.word, name: data.word };
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
                                            queue: false,
                                            message: "Choose a lineage for this word.",
                                            filterDone: function (data) {
                                                if (data.lin === "") return "Select a lineage or cancel.";
                                            }
                                        }).done(async function (data) {
                                            newObj.lineage = data.lin;
                                            await updaterecognitionobject({ name: mdname, obj: newObj });
                                            edited = true;
                                        });
                                    }
                                    else {
                                        await updaterecognitionobject({ name: mdname, obj: newObj });
                                        edited = true;
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
                    buttonDone: { add: "Add", terminus: "Create terminus" },
                    buttonFail: "Cancel",
                    queue: false,
                    message: "Add a word to the recognition tree.",
                    filterDone: function (data,button) {
                        if (data.word === "" && button !== "terminus") return "Supply a word";
                    }
                }).done(async function (data, button) {
                    try {
                        console.log('add node ' + data);
                        if (button === "terminus") {
                            await CreateRecognitionTerminusNode(evt);
                        }
                        else if (data.convLin) {
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
                                queue: false,
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
            edited = true;
        });
        recognitioncy.on('add', function (event) {
            node = event.target;
            if (!node.data('lineage')) {
                //realcy.remove(node);
                console.log(node);
            }
        });
    }
    catch (err) {
        HandleError(err);
    }
}


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
            input: div,
            queue: false
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
        message: "Create a new attribute",
        buttonDone: "Create",
        buttonFail: "Cancel",
        queue: false,
        filterDone: function (data) {
            if (data.name === "") return "Please supply a name";
            if (data.typeSelect === "") return "Please select a type";
            if (data.newLineage === "" && data.existinglineage === "") return "Choose an existing lineage or add a new word or lineage.";
        }
    }).done(async function (data) {
        //can only set value when creating, not existence
        var newAtt = {
            type: data.typeSelect, name: data.name, value: "", lineage: "", subLineage: "", properties: [] };
        if (data.existinglineage) {
            newAtt.lineage = data.existinglineage;
        }
        if (data.existingsublineage){
            newAtt.subLineage = data.existingsublineage;
        }
        if (newAtt.lineage === "") {
            var ivl = await isvalidlineage({ lin: data.newlineage });
            if (ivl.isvalidlineage) {
                newAtt.lineage = data.newlineage;
            }
        }
        if (newAtt.subLineage === "") {
            if (data.newsublineage !== "") {
                const ivl2 = await isvalidlineage({ lin: data.newsublineage });
                if (ivl2.isValidLineage) {
                    newAtt.subLineage = data.newsublineage;
                }
            }
        }
        if (newAtt.lineage === "" || (newAtt.subLineage === "" && data.newsublineage !== "")) {
            //at this point, if the lineages have been made from the dropdown or a valid lineage supplied, newAtt is correct.
            //only remaining alternative is that one or other is specified by a word or phrase.
            if (newAtt.lineage === "" && (newAtt.subLineage === "" && data.newsublineage !== "")) { //both
                let lineages = await getlineagesforword({ word: data.newlineage });
                let obj = {};
                $.each(lineages.getLineagesForWord, function (i, item) {
                    obj[item.lineage] = item.typeWord + ": " + item.description;
                });
                let sublineages = await getlineagesforword({ word: data.newsublineage });
                let subobj = {};
                $.each(sublineages.getLineagesForWord, function (i, item) {
                    subobj[item.lineage] = item.typeWord + ": " + item.description;
                });
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
                    queue: false,
                    message: "Choose primary and sub-lineage for these words.",
                    filterDone: function (data) {
                        if (data.lin === "" || data.sublin === "") return "Select lineages.";
                    }
                }).done(async function (data) {
                    newAtt.lineage = data.lin;
                    newAtt.subLineage = data.sublin;
                    await UpdateAttributeValue(id, newAtt, type);
                });
            }
            else if (newAtt.lineage === "" && !(newAtt.subLineage === "" && data.newsublineage !== "")) {//just lineage
                let lineages = await getlineagesforword({ word: data.newlineage });
                let obj = {};
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
                    queue: false,
                    filterDone: function (data) {
                        if (data.lin === "") return "Select a lineage or cancel.";
                    }
                }).done(async function (data) {
                    newAtt.lineage = data.lin;
                    await UpdateAttributeValue(id, newAtt, type);
                });
            }
            else if (newAtt.lineage !== "" && (newAtt.subLineage === "" && data.newsublineage !== "")) {//just sub lineage
                let lineages = await getlineagesforword({ word: data.newsublineage });
                let obj = {};
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
                    message: "Choose a sub-lineage for this word.",
                    queue: false,
                    filterDone: function (data) {
                        if (data.lin === "") return "Select a lineage or cancel.";
                    }
                }).done(async function (data) {
                    newAtt.subLineage = data.lin;
                    await UpdateAttributeValue(id, newAtt, type);
                });
            }
        }
        else {
            await UpdateAttributeValue(id, newAtt, type);
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
                var atts = {};
                $.each(obj.getGraphObjectById.properties, function (i, item) {
                    att[item.lineage] = item.name;
                    types[item.lineage] = item.type;
                    values[item.lineage] = item.value;
                    atts[item.lineage] = item;
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
                    queue: false,
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
                                edited = true;
                            }
                            catch (err) {
                                HandleError(err);
                            }
                        }
                    }
                    else {
                        var newAtt = atts[data.attChoice];
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

async function EditRecognitionMarkDown(id) {
    try {
        var obj = await recognitionobjectdata({ model: mdname, id: id });
        if (obj) {
            if (obj.getRecognitionObjectById.properties) {
                const text = obj.getRecognitionObjectById.properties.find(e => e.type === "MARKDOWN");
                if (text) {
                    await UpdateAttributeValue(id, text, "recognition");
                }
                else {
                    const newAtt = { value: "", lineage: textLineage, type: "MARKDOWN" };
                    await UpdateAttributeValue(id, newAtt, "recognition");
                }
            }
            else {
                const newAtt = { value: "", lineage: textLineage, type: "MARKDOWN" };
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
                    queue: false,
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
                            edited = true;
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
        edited = true;

    }
    catch (err) {
        HandleError(err);
    }
}

async function CreateRecognitionNode(evt, lin) {
    try {
        var name = lin;
        var typeword = await gettypeword({ lin: lin });
        if (typeword && typeword.getTypeWordForLineage !== "") {
            name = "~" + typeword.getTypeWordForLineage;
        }
        var created = await createrecognitionobject({ name: mdname, obj: { lineage: lin, name: name } });
        recognitioncy.add({
            group: 'nodes',
            data: { label: name, id: created.createRecognitionObject.id, lineage: lin },
            position: {
                x: evt.position.x,
                y: evt.position.y
            }
        });
        edited = true;
    }
    catch (err) {
        HandleError(err);
    }
}

async function CreateRecognitionTerminusNode(evt) {
    try {
        var name = "*";
        var created = await createrecognitionobject({ name: mdname, obj: { lineage: "terminus:", name: name } });
        recognitioncy.add({
            group: 'nodes',
            data: { label: name, id: created.createRecognitionObject.id, lineage: "terminus:" },
            position: {
                x: evt.position.x,
                y: evt.position.y
            }
        });
        edited = true;
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
        edited = true;
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
                    },
                    empty: {
                        type: "checkbox",
                        label: "Leave empty",
                        defaultValue: !(newAtt.value)
                    }

                },
                message: "Set the attribute's value",
                buttonDone: "Change",
                buttonFail: "Cancel",
                queue: false,
                filterDone: function (data) {
                    if (data.val === "" && !data.empty) return "Give a value or set to empty.";
                }
            }).done(function (valdata) {
                let content = valdata.empty ? "" : valdata.val;
                if (newAtt.value !== content || valdata.empty) {
                    newAtt.value = content;
                    Upsert(id, newAtt, type);
                }
            });
            break;
        case "CATEGORICAL":
            var currentCats = [];
            if (newAtt.properties) {
                newAtt.properties.forEach(function (v) {
                    if (v.name === "category") {
                        currentCats.push(v.value);
                    }
                });
            }

            $.MessageBox({
                input: {
                    newVal:
                    {
                        type: "select",
                        label: "categories",
                        options: currentCats,
                        defaultValue: newAtt.value
                    },
                    val: {
                        type: "text",
                        label: "new category",
                    },

                    empty: {
                        type: "checkbox",
                        label: "Leave empty",
                        defaultValue: !(newAtt.value)
                    }
                },
                message: "Choose a category",
                buttonDone: "Change",
                buttonFail: "Cancel",
                queue: false,
                filterDone: function (data) {
                    if (data.val === "" && data.newVal === "" && !data.empty) return "Give a value, new category or set to empty.";
                }
            }).done(function (valdata) {
                if (valdata.empty) { //Can add new category
                    if (valdata.val !== "" && !currentCats.includes(valdata.val)) {
                        if (!newAtt.properties) {
                            newAtt.properties = [];
                        }
                        newAtt.properties.push({
                            name: "category", lineage: "noun:01,0,0,15,07,02,02", type: "TEXTUAL", value: valdata.val
                        });
                    }
                    Upsert(id, newAtt, type);
                }
                else {
                    if (newAtt.value !== valdata.newVal && valdata.newVal !== "") {
                        newAtt.value = valdata.newVal;
                    }
                    if (valdata.val !== "" && !currentCats.includes(valdata.val)) {
                        if (newAtt.properties === null) {
                            newAtt.properties = [];
                        }
                        newAtt.properties.push({
                            name: "category", lineage: "noun:01,0,0,15,07,02,02", type: "TEXTUAL", value: valdata.val
                        });
                        if (valdata.newVal === "") {
                            newAtt.value = valdata.val;
                        }
                    }
                }
            });
            break;
        case "TEXTUAL":
            $.MessageBox({
                input: {
                    val: {
                        type: "text",
                        label: "Attribute value",
                        defaultValue: newAtt.value
                    },
                    empty: {
                        type: "checkbox",
                        label: "Leave empty",
                        defaultValue: !(newAtt.value)
                    }
                },
                message: "Set the attribute's value",
                buttonDone: "Change",
                buttonFail: "Cancel",
                queue: false,
                filterDone: function (data) {
                    if (data.val === "" && !data.empty) return "Give a value or set to empty.";
                }
            }).done(function (valdata) {
                let content = valdata.empty ? "" : valdata.val;
                if (newAtt.value !== content || valdata.empty) {
                    newAtt.value = content;
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
                    },
                    empty: {
                        type: "checkbox",
                        label: "Leave empty",
                        defaultValue: !(newAtt.value)
                    }

                },
                message: "Set the attribute's time value",
                buttonDone: "Change",
                buttonFail: "Cancel",
                queue: false,
                filterDone: function (data) {
                    if (data.date1 === "" && !data.empty) return "Give a value or set to empty.";
                }
            }).done(function (valData) {
                newAtt.value = valdata.empty ? "" : valdata.date1 + valdate.time1;
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
                queue: false,
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
                var alt = await defaultRule({ name: mdname, id: id, lineage: newAtt.lineage });
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
                queue: false,
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
        edited = true;
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

function HandleDates(existence, index) {
    if (existence === null)
        return null;
    if (existence.length <= index)
        return null;
    return existence[index].dateTime;
}

function HandleTimes(existence, index) {
    if (existence === null)
        return null;
    if (existence.length <= index)
        return null;
    var sep = existence[index].dateTimeOffset.indexOf('T');
    var res = existence[index].dateTimeOffset.substring(sep + 1, 9 + sep);
    return res;
}

// takes text from a chat message box or chat button press.
async function HandleChatText(text) {
    try {
        $('.write_msg').val('');
        AddOutGoingText(text);
        var res = await interact({ name: mdname, ksid: currentStateId, text: text });
        AddInComingMessage(res);
        $(".msg_history").stop().animate({ scrollTop: $(".msg_history")[0].scrollHeight }, 1000);
        await UpdateKS();
    }
    catch (err) {
        HandleError(err);
    }
}

function ClearChatText() {
    $('.msg_history').empty();
}

function ConvertHistoricDateTime(year, season) {
    let secondsPerYear = 31556952.0;
    if (season === null || season === "Winter") {
        return year * secondsPerYear;
    }
    else if (season === "Spring") {
        return year * secondsPerYear + secondsPerYear / 4.0;
    }
    else if (season === "Summer") {
        return year * secondsPerYear + secondsPerYear / 2.0;
    }
    return year * secondsPerYear + secondsPerYear * 0.75;
}

function ConvertRawToSeason(raw) {
    let secondsPerYear = 31556952.0;
    var sVal = raw % secondsPerYear;
    if (sVal > secondsPerYear * 0.75)
        return "Fall";
    if (sVal > secondsPerYear * 0.5)
        return "Summer";
    if (sVal > secondsPerYear * 0.25)
        return "Spring";
    return "Winter";
}

function ConvertRawToYear(raw) {
    let secondsPerYear = 31556952.0;
    return floor(raw / secondsPerYear);
}

async function UpdateKS() {
    try {
        const resp = await getks({ id: currentStateId, name: mdname });
        $('#kstate').jsonViewer(resp.getKnowledgeState, { collapsed: true, withQuotes: false, withLinks: true });
    }
    catch (err) {
        HandleError(err);
    }
}

async function EditKGDescription() {
    $.MessageBox({
        input: {
            val: {
                type: "markdown",
                defaultValue: descriptions[mdname],
                resize: true
            }
        },
        message: "Set the description",
        buttonDone: "Change",
        buttonFail: "Cancel",
        queue: false,
        filterDone: function (data) {
            if (data.val === "") return "Give a value.";
        }
    }).done(async function (valdata) {
        if (descriptions[mdname] !== valdata[0]) {
            descriptions[mdname] = valdata[0];
            try {
                await updatekg({ name: mdname, update: { description: valdata[0] } });
            }
            catch (err) {
                HandleError(err);
            }
        }
    });
}
