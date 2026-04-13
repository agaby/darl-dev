/// <summary>
/// </summary>

﻿using DarlCompiler.Ast;
using DarlCompiler.Interpreter;
using DarlCompiler.Interpreter.Ast;
using DarlCompiler.Parsing;
using DarlLanguage.Processing;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DarlLanguage
{
    /// <summary>
    /// Defines the Darl language and the interpreter.
    /// </summary>
    [Language("Darl", "1.3", "Darl language, Copyright(c) Dr Andy's IP Ltd, 2020")]
    public class DarlGrammar : InterpretedLanguageGrammar, ICanRunSample
    {


        /// <summary>
        /// Used to get values in and out of the map/ruleset
        /// </summary>
        public List<DarlResult> results { get; set; }

        /// <summary>
        /// Used to get values in and out of the map/ruleset
        /// </summary>
        public Dictionary<string, ILocalStore> stores { get; set; } = new Dictionary<string, ILocalStore>();

        /// <summary>
        /// If non-empty indicates that just the specified rule set is targeted for evaluation
        /// </summary>
        public string rulesetFilter { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public DarlGrammar() : base(true)
        {

            if (results == null)
                results = new List<DarlResult>();

            #region comments
            var singleLineComment = new CommentTerminal("SingleLineComment", "//", "\r", "\n", "\u2085", "\u2028", "\u2029");
            var delimitedComment = new CommentTerminal("DelimitedComment", "/*", "*/");
            var insertionComment = new CommentTerminal("DelimitedComment", "%%", "%%");
            NonGrammarTerminals.Add(singleLineComment);
            NonGrammarTerminals.Add(delimitedComment);
            NonGrammarTerminals.Add(insertionComment);
            #endregion


            #region keywords
            //Functions to add coloring to keywords/operators

            //These are provided only so that the editor can color the source


            var IF = Keyword("if", "Introduces a new rule");
            var ANYTHING = Keyword("anything", "ensures rule will always be valid");
            var IS = Keyword("is", "Determines the degree of truth of the statements surrounding it");
            var THEN = Keyword("then", "Introduces the output for the rule");
            var WILL = Keyword("will");
            var BE = Keyword("be");
            var CONFIDENCE = Keyword("confidence", "A value between 0 and 1 inclusive that assigns a plausibility to this rule. Default is 1.0");
            var INPUT = Keyword("input", "A variable provided from outside the rule set");
            var OUTPUT = Keyword("output", "A variable created by rules in this ruleset");
            var NUMERIC = Keyword("numeric", "A purely numeric variable");
            var CATEGORICAL = Keyword("categorical", "A variable that has distinct categories, like true/false, male/female etc.");
            var TEXTUAL = Keyword("textual", "A variable containing text that is not in distinct categories");
            var TEMPORAL = Keyword("temporal", "A variable containing a time value");
            var CONSTANT = Keyword("constant", "A numeric real valued constant.");
            var STRING = Keyword("string", "A string used for annotation or text processing.");
            var DURATION = Keyword("duration", "A time offset used for temporal processing.");
            var SUM = Keyword("sum", "Sums together a set of numeric values.");
            var PRODUCT = Keyword("product", "Multiplies together a set of numeric values.");
            var SIGMOID = Keyword("sigmoid", "Applies a Sigmoid logistic function to the supplied numeric value, and converts this to a degree of truth.");
            var DOCUMENT = Keyword("document", "Creates a textual output given a textual input and a set of parameters.");
            var NORMPROB = Keyword("normprob", "Applies a Gaussian probability density function to the value.");
            var ROUND = Keyword("round", "Rounds the first value to the limit of the second.");
            var MATCH = Keyword("match", "Applies a boolean returning regular expression, set by the string value, or sequence to the associated text input.");
            var AND = Keyword("and", "The logical 'and' of the operands either side, implemented as the minimum of their degrees of truth.");
            var OR = Keyword("or", "The logical 'or' of the operands either side, implemented as the maximum of their degrees of truth.");
            var NOT = Keyword("not", "The logical inverse of the operand after, implemented as 1 - the degree of truth.");
            var MAXIMUM = Keyword("maximum", "The maximum of a set of numeric values");
            var MINIMUM = Keyword("minimum", "The minimum of a set of numeric values");
            var FUZZYTUPLE = Keyword("fuzzytuple", "A set of numeric values in ascending order that delineate a fuzzy set or interval.");
            var RULESET = Keyword("ruleset", "A collection of rules and input and output definitions making up a functional inference block.");
            var WIRE = Keyword("wire", "A connection joining a map input or output to a rule set input or output, or between rule sets.");
            var MAPINPUT = Keyword("mapinput", "An outer level input to a map, consisting of one or more rule sets.");
            var MAPOUTPUT = Keyword("mapoutput", "An outer level output from a map, consisting of one or more rule sets.");
            var MAPSTORE = Keyword("mapstore", "An outer level store to and from a map, consisting of one or more rule sets.");
            var MINUSINFINITY = Keyword("-Infinity", "Represents negative infinity. Use to create sets unbounded on the lower side.");
            var PLUSINFINITY = Keyword("Infinity", "Represents positive infinity. Use to create sets unbounded on the upper side.");
            var MINTIME = Keyword("mintime", "Represents the start of representable time. Use to create sets unbounded on the lower side.");
            var MAXTIME = Keyword("maxtime", "Represents the end of representable time. Use to create sets unbounded on the upper side.");
            var NOW = Keyword("now", "Represents the current time and date.");
            var MULTIPLE = Keyword("multiple", "indicates the associated ruleset may return single or multiple results");
            var SHORTMINUSINFINITY = Keyword("-∞", "Represents negative infinity. Use to create sets unbounded on the lower side.");
            var SHORTPLUSINFINITY = Keyword("∞", "Represents positive infinity. Use to create sets unbounded on the upper side.");
            var PATTERN = Keyword("pattern", "Represents the address required to find data structures used for machine learning or inference.");
            var MANUAL = Keyword("manual", "The default state of a ruleset, this means that the rules inside the rule set are created by human intervention, as opposed to machine learning");
            var SUPERVISED = Keyword("supervised", "This means that the rules inside the rule set are created by supervised machine learning");
            var UNSUPERVISED = Keyword("unsupervised", "This means that the rules inside the rule set are created by unsupervised machine learning");
            var REINFORCEMENT = Keyword("reinforcement", "This means that the rules inside the rule set are created by reinforcement machine learning");
            var ASSOCIATION = Keyword("association", "This means that the rules inside the rule set are created by association machine learning");
            var DELAY = Keyword("delay", "A connection joining a map output to a rule set input which has a presettable fuzzy delay based on the time unit of the simulation.");
            var SEQUENCE = Keyword("sequence", "A sequence of entities matchable with an input array or string.");
            var RANDOMTEXT = Keyword("randomtext", "Randomly selects one of the input texts for output");
            var OTHERWISE = Keyword("otherwise", "Sets a rule that fires for the governing output if no other rules have fired.");
            var STORE = Keyword("store", "A store for associations that can be read and written to based on an index");
            var CATEGORYOF = Keyword("categoryof", "Converts a categorical input or outputs value to the matched categorical output where category names match");
            var TIMERANGE = Keyword("timerange", "A set of temporal values in sequential order that delineate a fuzzy set or interval.");
            var BEFORE = Keyword("before", "A comparison of two fuzzy times where the former wholly precedes the latter.");
            var PRECEDING = Keyword("preceding", "A comparison of two fuzzy times where the former precedes but joins with the latter.");
            var OVERLAPPING = Keyword("overlapping", "A comparison of two fuzzy times where the former and the latter are both current for a period");
            var DURING = Keyword("during", "A comparison of two fuzzy times where the former is completely contained within the latter");
            var STARTING = Keyword("starting", "A comparison of two fuzzy times where the former and the latter start at the same time.");
            var FINISHING = Keyword("finishing", "A comparison of two fuzzy times where the former and the latter end at the same time.");
            var AFTER = Keyword("after", "A comparison of two fuzzy times where the former wholly follows the latter.");
            var DYNAMIC = Keyword("dynamic", "indicates the categories will be filled at runtime");


            var EXISTS = Keyword("exists");
            var ABSENT = Keyword("absent");
            var PRESENT = Keyword("present");

            #endregion

            #region identifiers

            var textual_input = new DarlIdentifier("textual_input");
            var numeric_input = new DarlIdentifier("numeric_input");
            var categorical_input = new DarlIdentifier("categorical_input");
            var dynamic_categorical_input = new DarlIdentifier("dynamic_categorical_input");
            var temporal_input = new DarlIdentifier("temporal_input");
            var numeric_output = new DarlIdentifier("numeric_output");
            var categorical_output = new DarlIdentifier("categorical_output");
            var textual_output = new DarlIdentifier("textual_output");
            var temporal_output = new DarlIdentifier("temporal_output");
            var category = new DarlIdentifier("category");
            var set = new DarlIdentifier("set");
            var numeric_constant = new DarlIdentifier("numeric_constant");
            var string_constant = new DarlIdentifier("string_constant");
            var temporal_constant = new DarlIdentifier("temporal_constant");
            var sequence_constant = new DarlIdentifier("sequence_constant");
            var map_input = new DarlIdentifier("map_input");
            var map_output = new DarlIdentifier("map_output");
            var map_store = new DarlIdentifier("map_store");
            var rule_identifier = new DarlIdentifier("rule_identifier");
            var store_io = new DarlIdentifier("store_io");

            #endregion

            #region literals

            var numberLiteral = new NumberLiteral("numberLiteral", NumberOptions.AllowSign);
            var stringLiteral = TerminalFactory.CreateCSharpString("stringLiteral");
            var rulesetNameLiteral = TerminalFactory.CreateCSharpString("rulesetNameLiteral");
            var categoryLiteral = new StringLiteral("categoryLiteral", "\"", StringOptions.None, typeof(CategoryLiteralNode));
            var durationLiteral = new FreeTextLiteral("durationLiteral", ";");

            #endregion

            #region NonTerminal Declarations
            // definitions
            var inputdefinition = new NonTerminal("inputdefinition", typeof(InputDefinitionNode));
            var outputdefinition = new NonTerminal("outputdefinition", typeof(OutputDefinitionNode));
            var constantdefinition = new NonTerminal("constantdefinition", typeof(ConstantDefinitionNode));
            var durationdefinition = new NonTerminal("durationdefinition", typeof(DurationDefinitionNode));
            var stringdefinition = new NonTerminal("stringdefinition", typeof(StringDefinitionNode));
            var set_definition = new NonTerminal("set_definition", typeof(SetDefinitionNode));
            var set_definitions = new NonTerminal("set_definitions", typeof(ExpressionListNode));
            var category_definitions = new NonTerminal("category_definitions", typeof(ExpressionListNode));
            var category_option = new NonTerminal("category_option");
            var set_option = new NonTerminal("set_option");
            var storedefinition = new NonTerminal("storedefinition", typeof(StoreDefinitionNode));
            var store = new NonTerminal("store", typeof(StoreNode));
            var store_rhs = new NonTerminal("store_rhs");

            var rule = new NonTerminal("rule", typeof(RuleNode));
            var top_logical_op = new NonTerminal("top_logical_op");
            var arith_expression = new NonTerminal("arith_expression");
            var temporal_expression = new NonTerminal("temporal_expression");
            var arith_expression_list = new NonTerminal("arith_expression_list", typeof(ExpressionListNode));
            var logical_expression = new NonTerminal("logical_expression");
            var numberLiteral_list = new NonTerminal("numberLiteral_list", typeof(ExpressionListNode));
            var sequence_list = new NonTerminal("sequence_list", typeof(ExpressionListNode));
            var subsequence_list = new NonTerminal("subsequence_list", typeof(ExpressionListNode));
            var subsequence_choice = new NonTerminal("subsequence_choice");
            var rvalue_choice = new NonTerminal("rvalue_choice");
            var subsequence_contents = new NonTerminal("subsequence_contents", typeof(ExpressionListNode));
            var is_expression = new NonTerminal("is_expression", typeof(IsNode));
            var statement = new NonTerminal("statement");
            var statement_content = new NonTerminal("statement_content");
            var program_root = new NonTerminal("program_root", typeof(MapRootNode));
            var confidence = new NonTerminal("confidence", typeof(ConfidenceNode));
            var comparitives = new NonTerminal("comparitives");
            var tempcomparitives = new NonTerminal("tempcomparitives");
            var numeric_rvalue = new NonTerminal("numeric_rvalue");
            var textual_rvalue = new NonTerminal("textual_rvalue");
            var temporal_rvalue = new NonTerminal("temporal_rvalue");
            var parameter_list = new NonTerminal("parameter_list");
            var anyio_list = new NonTerminal("anyio_list", typeof(AnyIOListNode));
            var anyio = new NonTerminal("anyio");
            var categoricalio = new NonTerminal("categoricalio");
            var brackets = new NonTerminal("brackets");
            var logicalbrackets = new NonTerminal("logicalbrackets");
            var ruleset = new NonTerminal("ruleset", typeof(RuleSetNode));
            var rule_root = new NonTerminal("rule_root", typeof(RuleRootNode));
            var map_list = new NonTerminal("map_list");
            var mapinputdefinition = new NonTerminal("mapinputdefinition", typeof(MapInputDefinitionNode));
            var mapoutputdefinition = new NonTerminal("mapoutputdefinition", typeof(MapOutputDefinitionNode));
            var mapstoredefinition = new NonTerminal("mapstoredefinition", typeof(MapStoreDefinitionNode));
            var wiredefinition = new NonTerminal("wiredefinition", typeof(WireDefinitionNode));
            var patterndefinition = new NonTerminal("patterndefinition", typeof(PatternDefinitionNode));
            var setValue = new NonTerminal("setvalue");
            var categoryChoice = new NonTerminal("categorychoice");
            var pathchoice = new NonTerminal("pathchoice");
            var minetype = new NonTerminal("minetype", typeof(MineTypeNode));
            var minetypechoice = new NonTerminal("minetypechoice");
            var wirechoice = new NonTerminal("wirechoice");
            var wirenumericin = new NonTerminal("wirenumericin", typeof(CompIoNode));
            var wirecategoricin = new NonTerminal("wirecategoricin", typeof(CompIoNode));
            var wiretextualin = new NonTerminal("wiretextualin", typeof(CompIoNode));
            var wiretemporalin = new NonTerminal("wiretemporalin", typeof(CompIoNode));
            var wireinout = new NonTerminal("wireinout", typeof(CompIoNode));
            var wirenumericout = new NonTerminal("wirenumericout", typeof(CompIoNode));
            var wirecategoricout = new NonTerminal("wirecategoricout", typeof(CompIoNode));
            var wiretextualout = new NonTerminal("wiretextualout", typeof(CompIoNode));
            var wiretemporalout = new NonTerminal("wiretemporalout", typeof(CompIoNode));
            var wirenumericinternal = new NonTerminal("wirenumericinternal", typeof(CompIoNode));
            var wirecategoricinternal = new NonTerminal("wirecategoricinternal", typeof(CompIoNode));
            var wiretextualinternal = new NonTerminal("wiretextualinternal", typeof(CompIoNode));
            var wiretemporalinternal = new NonTerminal("wiretemporalinternal", typeof(CompIoNode));
            var wirestore = new NonTerminal("wirestore", typeof(CompIoNode));
            var compnumericinput = new NonTerminal("compnumericinput", typeof(CompIoNode));
            var compnumericoutput = new NonTerminal("compnumericoutput", typeof(CompIoNode));
            var compcategoryinput = new NonTerminal("compcategoryinput", typeof(CompIoNode));
            var compcategoryoutput = new NonTerminal("compcategoryoutput", typeof(CompIoNode));
            var comptextualinput = new NonTerminal("comptextualinput", typeof(CompIoNode));
            var comptextualoutput = new NonTerminal("comptextualoutput", typeof(CompIoNode));
            var comptemporalinput = new NonTerminal("comptemporalinput", typeof(CompIoNode));
            var comptemporaloutput = new NonTerminal("comptemporaloutput", typeof(CompIoNode));
            var compstore = new NonTerminal("compstore", typeof(CompIoNode));
            var absent = new NonTerminal("absent", typeof(AbsentNode));
            var present = new NonTerminal("present", typeof(PresentNode));
            var delaydefinition = new NonTerminal("delaydefinition", typeof(DelayDefinitionNode));
            var delaychoice = new NonTerminal("delaychoice");
            var sequencedefinition = new NonTerminal("sequencedefinition", typeof(SequenceDefinitionNode));
            var textmatchchoice = new NonTerminal("textmatchchoice");
            var textsourcechoice = new NonTerminal("textsourcechoice");
            var text_expression_list = new NonTerminal("text_expression_list", typeof(ExpressionListNode));
            var otherwise = new NonTerminal("otherwise", typeof(OtherwiseNode));
            var now = new NonTerminal("now", typeof(NowNode));
            var mintime = new NonTerminal("mintime", typeof(MinTimeNode));
            var maxtime = new NonTerminal("maxtime", typeof(MaxTimeNode));
            var timerange = new NonTerminal("timerange", typeof(TimeRangeNode));
            var bothCatInputTypes = new NonTerminal("bothCatInputTypes");


            this.Root = program_root;

            //operators
            var and = new NonTerminal("and", typeof(AndNode));
            var or = new NonTerminal("or", typeof(OrNode));
            var not = new NonTerminal("not", typeof(NotNode));
            var sigmoid = new NonTerminal("sigmoid", typeof(SigmoidNode));
            var sum = new NonTerminal("sum", typeof(SumNode));
            var fuzzytuple = new NonTerminal("fuzzytuple", typeof(FuzzytupleNode));
            var minimum = new NonTerminal("minimum", typeof(MinimumNode));
            var maximum = new NonTerminal("maximum", typeof(MaximumNode));
            var round = new NonTerminal("round", typeof(RoundNode));
            var match = new NonTerminal("match", typeof(MatchNode));
            var product = new NonTerminal("product", typeof(ProductNode));
            var eq = new NonTerminal("equal", typeof(EqualNode));
            var neq = new NonTerminal("not_equal", typeof(NotEqualNode));
            var tempeq = new NonTerminal("temp_equal", typeof(TempEqualNode));
            var tempneq = new NonTerminal("temp_not_equal", typeof(TempNotEqualNode));
            var plus = new NonTerminal("plus", typeof(PlusNode));
            var minus = new NonTerminal("minus", typeof(MinusNode));
            var power = new NonTerminal("power", typeof(PowerNode));
            var modulus = new NonTerminal("modulus", typeof(ModulusNode));
            var multiply = new NonTerminal("multiply", typeof(MultiplyNode));
            var divide = new NonTerminal("divide", typeof(DivideNode));
            var normprob = new NonTerminal("normprob", typeof(NormProbNode));
            var greater = new NonTerminal("greater", typeof(GreaterNode));
            var lesser = new NonTerminal("lesser", typeof(LesserNode));
            var gteq = new NonTerminal("greater_equal", typeof(GreaterEqualNode));
            var lseq = new NonTerminal("lesser_equal", typeof(LesserEqualNode));
            var anything = new NonTerminal("anything", typeof(AnythingNode));
            var document = new NonTerminal("document", typeof(DocumentNode));
            var randomtext = new NonTerminal("randomtext", typeof(RandomTextNode));
            var categoryof = new NonTerminal("categoryof", typeof(CategoryOfNode));
            var temporal_plus = new NonTerminal("temporal_plus", typeof(TemporalPlusNode));
            var temporal_minus = new NonTerminal("temporal_minus", typeof(TemporalMinusNode));
            var before = new NonTerminal("before", typeof(BeforeNode));
            var preceding = new NonTerminal("preceding", typeof(PrecedesNode));
            var overlapping = new NonTerminal("overlapping", typeof(OverlapsNode));
            var during = new NonTerminal("during", typeof(DuringNode));
            var starting = new NonTerminal("starting", typeof(StartsNode));
            var finishing = new NonTerminal("finishing", typeof(FinishesNode));
            var after = new NonTerminal("after", typeof(AfterNode));




            #endregion

            #region NonTerminal Rules

            program_root.Rule = MakeStarRule(program_root, null, map_list);
            program_root.ErrorRule = SyntaxError + ToTerm("{}");

            map_list.Rule = ruleset | mapinputdefinition | mapoutputdefinition | wiredefinition | patterndefinition | delaydefinition;



            patterndefinition.Rule = PATTERN + stringLiteral + ToTerm(";");

            mapinputdefinition.Rule = MAPINPUT + map_input + pathchoice + ToTerm(";");

            mapoutputdefinition.Rule = MAPOUTPUT + map_output + pathchoice + ToTerm(";");

            mapstoredefinition.Rule = MAPSTORE + map_store + ToTerm(";");

            wiredefinition.Rule = WIRE + wirechoice + ToTerm(";");


            wirenumericin.Rule = map_input + compnumericinput;

            wirecategoricin.Rule = map_input + compcategoryinput;

            wiretextualin.Rule = map_input + comptextualinput;

            wiretemporalin.Rule = map_input + comptemporalinput;

            wireinout.Rule = map_input + map_output;

            wirenumericout.Rule = compnumericoutput + map_output;

            wirecategoricout.Rule = compcategoryoutput + map_output;

            wiretextualout.Rule = comptextualoutput + map_output;

            wiretemporalout.Rule = comptemporaloutput + map_output;

            wirenumericinternal.Rule = compnumericoutput + compnumericinput;

            wirecategoricinternal.Rule = compcategoryoutput + compcategoryinput;

            wiretextualinternal.Rule = comptextualoutput + comptextualinput;

            wiretemporalinternal.Rule = comptemporaloutput + comptemporalinput;

            wirestore.Rule = map_store + compstore;

            wirechoice.Rule = wirenumericin | wirecategoricin | wiretextualin | wireinout | wirenumericout | wirecategoricout | wirenumericinternal | wirecategoricinternal | wiretextualout | wiretextualinternal | wirestore;

            ruleset.Rule = RULESET + rule_identifier + minetypechoice + "{" + rule_root + "}";

            minetypechoice.Rule = MANUAL | SUPERVISED | UNSUPERVISED | REINFORCEMENT | ASSOCIATION | MULTIPLE | Empty;

            compnumericinput.Rule = rule_identifier + PreferShiftHere() + "." + numeric_input;

            compnumericoutput.Rule = rule_identifier + PreferShiftHere() + "." + numeric_output;

            compcategoryinput.Rule = rule_identifier + PreferShiftHere() + "." + bothCatInputTypes;  //possible problems

            compcategoryoutput.Rule = rule_identifier + PreferShiftHere() + "." + categorical_output;

            comptextualinput.Rule = rule_identifier + PreferShiftHere() + "." + textual_input;

            comptextualoutput.Rule = rule_identifier + PreferShiftHere() + "." + textual_output;

            comptemporalinput.Rule = rule_identifier + PreferShiftHere() + "." + temporal_input;

            comptemporaloutput.Rule = rule_identifier + PreferShiftHere() + "." + temporal_output;

            compstore.Rule = rule_identifier + PreferShiftHere() + "." + store_io;

            rule_root.Rule = MakeStarRule(rule_root, null, statement);

            statement.Rule = statement_content + ToTerm(";");

            statement.ErrorRule = SyntaxError + ToTerm(";");

            statement_content.Rule = rule | inputdefinition | outputdefinition | constantdefinition | stringdefinition | sequencedefinition | storedefinition | durationdefinition | otherwise;

            confidence.Rule = Empty | (CONFIDENCE + numberLiteral);

            numeric_rvalue.Rule = set | arith_expression | store;

            temporal_rvalue.Rule = temporal_expression | store;


            textual_rvalue.Rule = textual_input | document | string_constant | randomtext | stringLiteral | store;

            rvalue_choice.Rule = textual_rvalue | numeric_rvalue | categoryChoice /*| temporal_rvalue*/;

            rule.Rule = (IF + top_logical_op + THEN + categorical_output + WILL + BE + categoryChoice + confidence) |
                (IF + top_logical_op + THEN + numeric_output + WILL + BE + numeric_rvalue + confidence) |
                (IF + top_logical_op + THEN + textual_output + WILL + BE + textual_rvalue + confidence) |
                (IF + top_logical_op + THEN + temporal_output + WILL + BE + temporal_rvalue + confidence) |
                (IF + top_logical_op + THEN + store + WILL + BE + textual_rvalue + confidence) |
                (IF + top_logical_op + THEN + store + WILL + BE + arith_expression + confidence) |
                (IF + top_logical_op + THEN + store + WILL + BE + categorical_input + confidence) |
                (IF + top_logical_op + THEN + store + WILL + BE + dynamic_categorical_input + confidence);

            otherwise.Rule = (OTHERWISE + IF + top_logical_op + THEN + categorical_output + WILL + BE + categoryChoice + confidence) |
                (OTHERWISE + IF + top_logical_op + THEN + numeric_output + WILL + BE + numeric_rvalue + confidence) |
                (OTHERWISE + IF + top_logical_op + THEN + textual_output + WILL + BE + textual_rvalue + confidence) |
                (OTHERWISE + IF + top_logical_op + THEN + temporal_output + WILL + BE + temporal_rvalue + confidence) |
                (OTHERWISE + IF + top_logical_op + THEN + store + WILL + BE + textual_rvalue + confidence) |
                (OTHERWISE + IF + top_logical_op + THEN + store + WILL + BE + arith_expression + confidence) |
                (OTHERWISE + IF + top_logical_op + THEN + store + WILL + BE + categorical_input + confidence);

            top_logical_op.Rule = anything | logical_expression;

            logical_expression.Rule = and | or | not | is_expression | logicalbrackets;

            logicalbrackets.Rule = "(" + logical_expression + ")";

            and.Rule = logical_expression + PreferShiftHere() + AND + logical_expression;

            or.Rule = logical_expression + PreferShiftHere() + OR + logical_expression;

            not.Rule = NOT + logical_expression;

            comparitives.Rule = eq | neq | greater | lesser | gteq | lseq;

            tempcomparitives.Rule = before | preceding | overlapping | during | starting | finishing | after | tempeq | tempneq;

            is_expression.Rule = ((categorical_input + IS + categoryChoice) |
                                        (categorical_output + IS + categoryChoice) |
                                        (numeric_input + IS + set) |
                                        (numeric_output + IS + set) |
                                        (numeric_input + IS + comparitives) |
                                        (numeric_output + IS + comparitives) |
                                        (temporal_input + IS + set) |
                                        (temporal_output + IS + set) |
                                        (temporal_input + IS + tempcomparitives) |
                                        (temporal_output + IS + tempcomparitives) |
                                        (textual_input + IS + match) |
                                        (numeric_input + IS + absent) |
                                        (categorical_output + IS + absent) |
                                        (categorical_input + IS + absent) |
                                        (dynamic_categorical_input + IS + absent) |
                                        (numeric_output + IS + absent) |
                                        (textual_input + IS + absent) |
                                        (numeric_input + IS + present) |
                                        (temporal_input + IS + absent) |
                                        (temporal_output + IS + absent) |
                                        (temporal_input + IS + present) |
                                        (temporal_output + IS + present) |
                                        (categorical_output + IS + present) |
                                        (categorical_input + IS + present) |
                                        (dynamic_categorical_input + IS + present) |
                                        (numeric_output + IS + present) |
                                        (textual_input + IS + present) |
                                        (textual_output + IS + present) |
                                        (store + IS + comparitives) |
                                        (store + IS + match) |
                                        (store + IS + absent) |
                                        (store + IS + present)
                                        );


            eq.Rule = "=" + arith_expression;
            neq.Rule = "!=" + arith_expression;
            greater.Rule = ">" + arith_expression;
            lesser.Rule = "<" + arith_expression;
            gteq.Rule = ">=" + arith_expression;
            lseq.Rule = "<=" + arith_expression;

            tempeq.Rule = "=" + temporal_expression;
            tempneq.Rule = "!=" + temporal_expression;
            before.Rule = BEFORE + temporal_expression;
            preceding.Rule = PRECEDING + temporal_expression;
            overlapping.Rule = OVERLAPPING + temporal_expression;
            during.Rule = DURING + temporal_expression;
            finishing.Rule = FINISHING + temporal_expression;
            starting.Rule = STARTING + temporal_expression;
            after.Rule = AFTER + temporal_expression;



            anything.Rule = ANYTHING;

            brackets.Rule = "(" + arith_expression + ")";

            plus.Rule = arith_expression + PreferShiftHere() + "+" + arith_expression;

            minus.Rule = arith_expression + PreferShiftHere() + "-" + arith_expression;

            multiply.Rule = arith_expression + PreferShiftHere() + "*" + arith_expression;

            divide.Rule = arith_expression + PreferShiftHere() + "/" + arith_expression;

            modulus.Rule = arith_expression + PreferShiftHere() + "%" + arith_expression;

            power.Rule = arith_expression + PreferShiftHere() + "^" + arith_expression;

            arith_expression.Rule = plus | minus | multiply | divide | modulus | power | numeric_constant | numeric_input | numeric_output | sum | product | fuzzytuple | minimum | maximum | sigmoid | normprob | round | brackets | numberLiteral;

            temporal_plus.Rule = temporal_expression + PreferShiftHere() + "+" + temporal_constant | temporal_constant + PreferShiftHere() + "+" + temporal_expression;

            temporal_minus.Rule = temporal_expression + PreferShiftHere() + "-" + temporal_constant;

            temporal_expression.Rule = temporal_plus | temporal_minus | temporal_input | temporal_output | now | mintime | maxtime;

            category_option.Rule = Empty | "{" + category_definitions + "}";

            set_option.Rule = Empty | "{" + set_definitions + "}";

            inputdefinition.Rule = (INPUT + NUMERIC + numeric_input + set_option) |
                       (INPUT + CATEGORICAL + categorical_input + category_option) |
                       (INPUT + DYNAMIC + CATEGORICAL + dynamic_categorical_input + store) |
                       (INPUT + TEXTUAL + textual_input) |
                       (INPUT + TEMPORAL + temporal_input);

            outputdefinition.Rule = (OUTPUT + NUMERIC + numeric_output + set_option) |
                       (OUTPUT + CATEGORICAL + categorical_output + category_option) |
                       (OUTPUT + TEXTUAL + textual_output) |
                       (OUTPUT + TEMPORAL + temporal_output);

            constantdefinition.Rule = CONSTANT + numeric_constant + numberLiteral;

            durationdefinition.Rule = DURATION + temporal_constant + durationLiteral;

            stringdefinition.Rule = STRING + string_constant + stringLiteral;

            storedefinition.Rule = STORE + store_io;

            parameter_list.Rule = "(" + arith_expression_list + ")";

            store_rhs.Rule = "[" + text_expression_list + "]";

            store.Rule = store_io + store_rhs;

            sum.Rule = SUM + parameter_list;

            product.Rule = PRODUCT + parameter_list;

            fuzzytuple.Rule = FUZZYTUPLE + parameter_list;

            minimum.Rule = MINIMUM + parameter_list;

            maximum.Rule = MAXIMUM + parameter_list;

            sigmoid.Rule = SIGMOID + "(" + arith_expression + ")";

            normprob.Rule = NORMPROB + "(" + arith_expression + ")";

            round.Rule = ROUND + "(" + arith_expression + "," + arith_expression + ")";

            anyio.Rule = numeric_input | numeric_output | categorical_input | categorical_output | textual_input | textual_output | temporal_input | temporal_output | dynamic_categorical_input;

            anyio_list.Rule = MakePlusRule(anyio_list, ToTerm(","), anyio);

            categoricalio.Rule = categorical_input | categorical_output | dynamic_categorical_input;

            arith_expression_list.Rule = MakePlusRule(arith_expression_list, ToTerm(","), arith_expression);

            numberLiteral_list.Rule = MakePlusRule(numberLiteral_list, ToTerm(","), setValue);

            setValue.Rule = numberLiteral | MINUSINFINITY | PLUSINFINITY | SHORTMINUSINFINITY | SHORTPLUSINFINITY;

            textmatchchoice.Rule = string_constant | sequence_constant | stringLiteral;

            match.Rule = MATCH + textmatchchoice;

            set_definitions.Rule = MakePlusRule(set_definitions, ToTerm(","), set_definition);

            set_definition.Rule = "{" + set + "," + numberLiteral_list + "}";

            category_definitions.Rule = MakePlusRule(category_definitions, ToTerm(","), categoryChoice);

            categoryChoice.Rule = categoryLiteral | category | categoryof | Empty;

            pathchoice.Rule = stringLiteral | Empty;

            absent.Rule = ABSENT;

            present.Rule = PRESENT;

            delaychoice.Rule = compnumericinput | compcategoryinput;

            delaydefinition.Rule = DELAY + map_output + delaychoice + "{" + numberLiteral_list + "}" + ToTerm(";");

            subsequence_contents.Rule = MakePlusRule(subsequence_contents, ToTerm(","), stringLiteral);

            subsequence_list.Rule = "{" + subsequence_contents + "}";

            subsequence_choice.Rule = stringLiteral | subsequence_list;

            sequence_list.Rule = MakePlusRule(sequence_list, ToTerm(","), subsequence_choice);

            sequencedefinition.Rule = SEQUENCE + sequence_constant + "{" + sequence_list + "}";

            textsourcechoice.Rule = stringLiteral | textual_input | string_constant | textual_output | store | dynamic_categorical_input;

            document.Rule = DOCUMENT + "(" + textsourcechoice + "," + "{" + anyio_list + "}" + ")";

            text_expression_list.Rule = MakePlusRule(text_expression_list, ToTerm(","), textsourcechoice);

            randomtext.Rule = RANDOMTEXT + "(" + text_expression_list + ")";

            categoryof.Rule = CATEGORYOF + "(" + categoricalio + ")";

            now.Rule = NOW;

            mintime.Rule = MINTIME;

            maxtime.Rule = MAXTIME;

            timerange.Rule = TIMERANGE + parameter_list;

            bothCatInputTypes.Rule = (categorical_input | dynamic_categorical_input);



            #endregion

            RegisterOperators(1, "or");
            RegisterOperators(2, "and");
            RegisterOperators(3, "+", "-");
            RegisterOperators(3, "not");
            RegisterOperators(4, "*", "/", "%");
            RegisterOperators(4, IS);
            RegisterOperators(5, "^");
            RegisterOperators(6, ".");

            MarkPunctuation("{", "}", "(", ")", "[", "]", ",", ";", ".", "if", "then", "will", "be", "confidence", "input", "output", "numeric", "categorical", "arity", "presence", "string", "constant", "or", "and", "not", "is", "*", "/", "-", "+", "%", "^", ">", "<", "<=", ">=", "anything", "textual", "maximum", "minimum", "sum", "product", "fuzzytuple", "sigmoid", "normprob", "round", "ruleset", "wire", "mapinput", "mapoutput", "pattern", "absent", "present", "delay", "sequence", "match", "document", "randomtext", "store", "temporal", "categoryof", "duration", "now", "mintime", "maxtime", "after", "before", "preceding", "overlapping", "during", "finishing", "starting", "dynamic");
            RegisterBracePair("(", ")");
            RegisterBracePair("{", "}");
            RegisterBracePair("[", "]");
            MarkTransient(top_logical_op, statement_content, category_option, logical_expression, set_option,
                numeric_rvalue, arith_expression, statement, comparitives, tempcomparitives, parameter_list, brackets, setValue,
                categoryChoice, pathchoice, minetypechoice, wirechoice, map_list, delaychoice, subsequence_choice,
                textmatchchoice, anyio, textual_rvalue, textsourcechoice, logicalbrackets, rvalue_choice, store_rhs, temporal_rvalue,
                temporal_expression, categoricalio, bothCatInputTypes);

            LanguageFlags = LanguageFlags.CreateAst; // LanguageFlags.Default;// // | DarlCompiler.Parsing.LanguageFlags.NewLineBeforeEOF;   
        }

        //Must create new overrides here in order to support the "Operator" token color
        /// <summary>
        /// Registers the operators.
        /// </summary>
        /// <param name="precedence">The precedence.</param>
        /// <param name="opSymbols">The op symbols.</param>
        public new void RegisterOperators(int precedence, params string[] opSymbols)
        {
            RegisterOperators(precedence, Associativity.Left, opSymbols);
        }

        /// <summary>
        /// Gets the specified keyword.
        /// </summary>
        /// <param name="keyword">The keyword.</param>
        /// <returns>The Keyword</returns>
        public KeyTerm Keyword(string keyword)
        {
            return Keyword(keyword, string.Empty);
        }

        /// <summary>
        /// Keywords the specified keyword.
        /// </summary>
        /// <param name="keyword">The keyword.</param>
        /// <param name="tooltip">The tooltip.</param>
        /// <returns>The KeyTerm</returns>
        public KeyTerm Keyword(string keyword, string tooltip)
        {
            var term = ToTerm(keyword);
            this.MarkReservedWords(keyword);
            term.EditorInfo = new TokenEditorInfo(TokenType.Keyword, TokenColor.Keyword, TokenTriggers.None);
            if (tooltip != string.Empty)
                term.EditorInfo.ToolTip = tooltip;
            return term;
        }

        /// <summary>
        /// Operators the specified op.
        /// </summary>
        /// <param name="op">The op.</param>
        /// <returns>The KeyTerm</returns>
        public KeyTerm Operator(string op)
        {
            string opCased = this.CaseSensitive ? op : op.ToLower();
            var term = new KeyTerm(opCased, op);
            return term;
        }

        /// <summary>
        /// Builds the ast.
        /// </summary>
        /// <param name="language">The language.</param>
        /// <param name="parseTree">The parse tree.</param>
        public override void BuildAst(LanguageData language, ParseTree parseTree, Dictionary<string, ILocalStore> stores)
        {
            if (!LanguageFlags.IsSet(LanguageFlags.CreateAst))
                return;
            var astContext = new AstContext(language, stores);
            astContext.DefaultIdentifierNodeType = typeof(DarlIdentifierNode);
            astContext.DefaultLiteralNodeType = typeof(DarlNumberLiteralNode);
            var astBuilder = new AstBuilder(astContext);
            astBuilder.BuildAst(parseTree);
            //auto-wire if necessary. Conditions are 1 ruleset, no mapinputs or outputs.
            var root = parseTree.GetMapRoot();
            if (root.rulesets.Count == 1 && parseTree.GetMapOutputs().Count == 0 && parseTree.GetMapInputs().Count == 0)
            {
                root.wires.Clear(); //ignore any existing wires.
                var ruleset = root.rulesets.First().Value;
                var name = ruleset.rulesetname;
                foreach (var inp in ruleset.ruleRoot.inputs.Values)
                {
                    root.inputs.Add(inp.name, new MapInputDefinitionNode { Name = inp.name, Parent = root });
                    root.wires.Add(new WireDefinitionNode { destname = inp.name, destRuleset = name, sourcename = inp.name, wiretype = WireDefinitionNode.WireType.wirein, Parent = root });
                }
                foreach (var outp in ruleset.ruleRoot.outputs.Values.Where(a => a is OutputDefinitionNode))
                {
                    root.outputs.Add(outp.name, new MapOutputDefinitionNode { Name = outp.GetName(), Parent = root });
                    root.wires.Add(new WireDefinitionNode { destname = outp.name, sourcename = outp.name, sourceRuleset = name, wiretype = WireDefinitionNode.WireType.wireout, Parent = root });
                }
                foreach (var store in ruleset.ruleRoot.stores.Values)
                {
                    root.stores.Add(store.name, new MapStoreDefinitionNode { Name = store.name, Parent = root });
                    root.wires.Add(new WireDefinitionNode { destname = store.name, sourcename = store.name, sourceRuleset = name, wiretype = WireDefinitionNode.WireType.wirestore, Parent = root });
                }
            }

        }

        /// <summary>
        /// Runs the sample.
        /// </summary>
        /// <param name="args">The args.</param>
        /// <returns>logged data</returns>
        public override async Task<string> RunSample(RunSampleArgs args)
        {
            rulesetFilter = args.Sample;
            var res =  await base.RunSample(args);
            return res.ToString();
        }

        public DarlResult ResultByName(string name)
        {
            if (results.Any(a => a.name == name))
            {
                return results.First(a => a.name == name);
            }
            return null;
        }

        public DarlResult LastResultByName(string name)
        {
            if (results.Any(a => a.name == name))
            {
                return results.Last(a => a.name == name);
            }
            return null;
        }
    }
}
