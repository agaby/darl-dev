//functions for adding darl functionality to CodeMirror
(function (mod) {
    if (typeof exports == "object" && typeof module == "object") // CommonJS
        mod(require("../../lib/codemirror"));
    else if (typeof define == "function" && define.amd) // AMD
        define(["../../lib/codemirror"], mod);
    else // Plain browser env
        mod(CodeMirror);
})

    (function (CodeMirror) {
        "use strict";



        CodeMirror.remoteValidator = function (text, updateLinting, options, cm) {

            if (text.trim() == "") {
                updateLinting(cm, []);
                return;
            }

            function result_cb(error_list) {
                var found = [];

                for (var i in error_list) {
                    var error = error_list[i];

                    var start_line = error.line_no;

                    var start_char;
                    if (typeof (error.column_no_start) != "undefined")
                        start_char = error.column_no_start;
                    else
                        start_char = error.column_no;

                    var end_char;
                    if (typeof (error.column_no_stop) != "undefined")
                        end_char = error.column_no_stop;
                    else
                        end_char = error.column_no;

                    var end_line = error.line_no;
                    var message = error.message;

                    var severity;
                    if (typeof (error.severity) != "undefined")
                        severity = error.severity;
                    else
                        severity = 'error';

                    found.push({
                        from: CodeMirror.Pos(start_line - 1, start_char),
                        to: CodeMirror.Pos(end_line - 1, end_char),
                        message: message,
                        severity: severity // "error", "warning"
                    });
                }

                updateLinting(cm, found);
            }

            options.check_cb(text, result_cb)
        }

        CodeMirror.registerHelper("hint", "darl", function (editor, options) {
            var word = /[\w$]+/;
            var cur = editor.getCursor(), curLine = editor.getLine(cur.line);
            var end = cur.ch, start = end;
            while (start && word.test(curLine.charAt(start - 1)))--start;
            var curWord = start != end && curLine.slice(start, end);
            var token = editor.getTokenAt(cur);
            var prevToken = editor.getTokenAt(CodeMirror.Pos(cur.line, token.start));

            var found = [];
            //if previous token is one of these the choices are known
            switch (prevToken.string) {
                case "if":
                    found.push({ text: "anything", displayText: "anything" });
                    found.push({ text: "", displayText: "input name" });
                    found.push({ text: "", displayText: "output name" });
                    break;
                case "anything":
                    found.push({ text: "then", displayText: "then" });
                    break;
                case "then":
                    found.push({ text: "", displayText: "output name" });
                    break;
                case "will":
                    found.push({ text: "be", displayText: "be" });
                    break;
                case "input":
                    found.push({ text: "numeric", displayText: "numeric" });
                    found.push({ text: "categorical", displayText: "categorical" });
                    found.push({ text: "textual;", displayText: "textual" });
                    found.push({ text: "temporal;", displayText: "temporal" });
                    found.push({ text: "dynamic;", displayText: "temporal" });
                    break;
                case "output":
                    found.push({ text: "numeric", displayText: "numeric" });
                    found.push({ text: "categorical", displayText: "categorical" });
                    found.push({ text: "textual;", displayText: "textual" });
                    found.push({ text: "temporal;", displayText: "temporal" });
                    break;
                case "confidence":
                    found.push({ text: "1.0;", displayText: "number between 0.0 and 1.0" });
                    break;
                case "numeric":
                    found.push({ text: "name {{low, 0,1}, {high, 1,2}};", displayText: "name + optional list of sets" });
                    break;
                case "categorical":
                    found.push({ text: "name {True,False};", displayText: "name + optional list of categories" });
                    break;
                case "constant":
                    found.push({ text: "Zero 0;", displayText: "name + numerical value" });
                    break;
                case "duration":
                    found.push({ text: "week 7.00:00:0.0;", displayText: "name + time offset value" });
                    break;
                case "string":
                    found.push({ text: "stringName \U+0022 \U+0022 ;", displayText: "name + text in inverted commas" });
                    break;
                case "sequence":
                    found.push({ text: "seqName;", displayText: "name + a jagged array of items" });
                    break;
                case "otherwise":
                    found.push({ text: "if", displayText: "if" });
                    break;
                case "store":
                    found.push({ text: "storename ;", displayText: "name of store" });
                    break;
                case "dynamic":
                    found.push({ text: "categorical ;", displayText: "categorical" });
                    break;           }
            if (found.length > 0)
                return { list: found, from: CodeMirror.Pos(cur.line, start), to: CodeMirror.Pos(cur.line, end) };
            // determine if within a block or not
            var ctx = token.state.context;
            while (ctx != null && ctx.type != "}" && ctx.type != "top")
                ctx = ctx.prev;
            if (ctx != null && ctx.type == "}") {
                // return the inner list.
                var list = ("if anything is then will be confidence input output numeric categorical textual constant string " +
                    "sum product sigmoid normprob round match and or not maximum minimum fuzzytuple " +
                    "exists absent present sequence document randomtext otherwise store duration temporal" +
                    "categoryof timerange before preceding overlapping during starting finishing after now dynamic").split(" ");

                for (var i = 0; i < list.length; i++) {
                    var word = list[i];
                    if (word.slice(0, curWord.length) == curWord)
                        found.push({ text: word, displayText: word });
                }

                return { list: found, from: CodeMirror.Pos(cur.line, start), to: CodeMirror.Pos(cur.line, end) };
            }
            else {//return the outer list
                var list = ("ruleset wire mapinput mapoutput pattern delay").split(" ");

                for (var i = 0; i < list.length; i++) {
                    var word = list[i];
                    if (word.slice(0, curWord.length) == curWord)
                        found.push({ text: word, displayText: word });
                }
                return { list: found, from: CodeMirror.Pos(cur.line, start), to: CodeMirror.Pos(cur.line, end) };
            }
        });

        CodeMirror.defineMode("clike", function (config, parserConfig) {
            var indentUnit = config.indentUnit,
                statementIndentUnit = parserConfig.statementIndentUnit || indentUnit,
                dontAlignCalls = parserConfig.dontAlignCalls,
                keywords = parserConfig.keywords || {},
                builtin = parserConfig.builtin || {},
                blockKeywords = parserConfig.blockKeywords || {},
                atoms = parserConfig.atoms || {},
                hooks = parserConfig.hooks || {},
                multiLineStrings = parserConfig.multiLineStrings,
                indentStatements = parserConfig.indentStatements !== false;
            var isOperatorChar = /[+\-*&%=<>!?|\/]/;

            var curPunc;

            function tokenBase(stream, state) {
                var ch = stream.next();
                if (hooks[ch]) {
                    var result = hooks[ch](stream, state);
                    if (result !== false) return result;
                }
                if (ch == '"' || ch == "'") {
                    state.tokenize = tokenString(ch);
                    return state.tokenize(stream, state);
                }
                if (/[\[\]{}\(\),;\:\.]/.test(ch)) {
                    curPunc = ch;
                    return null;
                }
                if (/\d/.test(ch)) {
                    stream.eatWhile(/[\w\.]/);
                    return "number";
                }
                if (ch == "/") {
                    if (stream.eat("*")) {
                        state.tokenize = tokenComment;
                        return tokenComment(stream, state);
                    }
                    if (stream.eat("/")) {
                        stream.skipToEnd();
                        return "comment";
                    }
                }
                if (ch == "%") {
                    if (stream.eat("%")) {
                        state.tokenize = tokenComment;
                        return insertComment(stream, state);
                    }
                }
                if (isOperatorChar.test(ch)) {
                    stream.eatWhile(isOperatorChar);
                    return "operator";
                }
                stream.eatWhile(/[\w\$_\xa1-\uffff]/);
                var cur = stream.current();
                if (keywords.propertyIsEnumerable(cur)) {
                    if (blockKeywords.propertyIsEnumerable(cur)) curPunc = "newstatement";
                    return "keyword";
                }
                if (builtin.propertyIsEnumerable(cur)) {
                    if (blockKeywords.propertyIsEnumerable(cur)) curPunc = "newstatement";
                    return "builtin";
                }
                if (atoms.propertyIsEnumerable(cur)) return "atom";
                return "variable";
            }

            function tokenString(quote) {
                return function (stream, state) {
                    var escaped = false, next, end = false;
                    while ((next = stream.next()) != null) {
                        if (next == quote && !escaped) { end = true; break; }
                        escaped = !escaped && next == "\\";
                    }
                    if (end || !(escaped || multiLineStrings))
                        state.tokenize = null;
                    return "string";
                };
            }

            function tokenComment(stream, state) {
                var maybeEnd = false, ch;
                while (ch = stream.next()) {
                    if (ch == "/" && maybeEnd) {
                        state.tokenize = null;
                        break;
                    }
                    maybeEnd = (ch == "*");
                }
                return "comment";
            }

            function insertComment(stream, state) {
                var maybeEnd = false, ch;
                while (ch = stream.next()) {
                    if (ch == "%" && maybeEnd) {
                        state.tokenize = null;
                        break;
                    }
                    maybeEnd = (ch == "%");
                }
                return "comment";
            }

            function Context(indented, column, type, align, prev) {
                this.indented = indented;
                this.column = column;
                this.type = type;
                this.align = align;
                this.prev = prev;
            }
            function pushContext(state, col, type) {
                var indent = state.indented;
                if (state.context && state.context.type == "statement")
                    indent = state.context.indented;
                return state.context = new Context(indent, col, type, null, state.context);
            }
            function popContext(state) {
                var t = state.context.type;
                if (t == ")" || t == "]" || t == "}")
                    state.indented = state.context.indented;
                return state.context = state.context.prev;
            }

            // Interface

            return {
                startState: function (basecolumn) {
                    return {
                        tokenize: null,
                        context: new Context((basecolumn || 0) - indentUnit, 0, "top", false),
                        indented: 0,
                        startOfLine: true
                    };
                },

                token: function (stream, state) {
                    var ctx = state.context;
                    if (stream.sol()) {
                        if (ctx.align == null) ctx.align = false;
                        state.indented = stream.indentation();
                        state.startOfLine = true;
                    }
                    if (stream.eatSpace()) return null;
                    curPunc = null;
                    var style = (state.tokenize || tokenBase)(stream, state);
                    if (style == "comment" || style == "meta") return style;
                    if (ctx.align == null) ctx.align = true;

                    if ((curPunc == ";" || curPunc == ":" || curPunc == ",") && ctx.type == "statement") popContext(state);
                    else if (curPunc == "{") pushContext(state, stream.column(), "}");
                    else if (curPunc == "[") pushContext(state, stream.column(), "]");
                    else if (curPunc == "(") pushContext(state, stream.column(), ")");
                    else if (curPunc == "}") {
                        while (ctx.type == "statement") ctx = popContext(state);
                        if (ctx.type == "}") ctx = popContext(state);
                        while (ctx.type == "statement") ctx = popContext(state);
                    }
                    else if (curPunc == ctx.type) popContext(state);
                    else if (indentStatements &&
                        (((ctx.type == "}" || ctx.type == "top") && curPunc != ';') ||
                            (ctx.type == "statement" && curPunc == "newstatement")))
                        pushContext(state, stream.column(), "statement");
                    state.startOfLine = false;
                    return style;
                },

                indent: function (state, textAfter) {
                    if (state.tokenize != tokenBase && state.tokenize != null) return CodeMirror.Pass;
                    var ctx = state.context, firstChar = textAfter && textAfter.charAt(0);
                    if (ctx.type == "statement" && firstChar == "}") ctx = ctx.prev;
                    var closing = firstChar == ctx.type;
                    if (ctx.type == "statement") return ctx.indented + (firstChar == "{" ? 0 : statementIndentUnit);
                    else if (ctx.align && (!dontAlignCalls || ctx.type != ")")) return ctx.column + (closing ? 0 : 1);
                    else if (ctx.type == ")" && !closing) return ctx.indented + statementIndentUnit;
                    else return ctx.indented + (closing ? 0 : indentUnit);
                },

                electricChars: "{}",
                blockCommentStart: "/*",
                blockCommentEnd: "*/",
                lineComment: "//",
                fold: "brace"
            };
        });

        function words(str) {
            var obj = {}, words = str.split(" ");
            for (var i = 0; i < words.length; ++i) obj[words[i]] = true;
            return obj;
        }

        // C#-style strings where "" escapes a quote.
        function tokenAtString(stream, state) {
            var next;
            while ((next = stream.next()) != null) {
                if (next == '"' && !stream.eat('"')) {
                    state.tokenize = null;
                    break;
                }
            }
            return "string";
        }

        // C++11 raw string literal is <prefix>"<delim>( anything )<delim>", where
        // <delim> can be a string up to 16 characters long.
        function tokenRawString(stream, state) {
            // Escape characters that have special regex meanings.
            var delim = state.cpp11RawStringDelim.replace(/[^\w\s]/g, '\\$&');
            var match = stream.match(new RegExp(".*?\\)" + delim + '"'));
            if (match)
                state.tokenize = null;
            else
                stream.skipToEnd();
            return "string";
        }

        function def(mimes, mode) {
            if (typeof mimes == "string") mimes = [mimes];
            var words = [];
            function add(obj) {
                if (obj) for (var prop in obj) if (obj.hasOwnProperty(prop))
                    words.push(prop);
            }
            add(mode.keywords);
            add(mode.builtin);
            add(mode.atoms);
            if (words.length) {
                mode.helperType = mimes[0];
                CodeMirror.registerHelper("hintWords", mimes[0], words);
            }

            for (var i = 0; i < mimes.length; ++i)
                CodeMirror.defineMIME(mimes[i], mode);
        }


        def("text/x-darl", {
            name: "clike",
            keywords: words("if anything is then will be confidence input output numeric categorical textual constant string " +
                "sum product sigmoid normprob round match and or not maximum minimum fuzzytuple " +
                "ruleset wire mapinput mapoutput pattern delay exists absent present sequence supervised document randomtext otherwise store duration temporal" +
                "categoryof timerange before preceding overlapping during starting finishing after now dynamic"
            ),
            blockKeywords: words("ruleset"),
            atoms: words("true false null"),
            hooks: {
                "@": function (stream) {
                    stream.eatWhile(/[\w\$_]/);
                    return "meta";
                }
            },
            modeProps: { fold: ["brace", "import"] }
        });

    });


