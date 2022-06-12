using Darl.Common;
using Darl_standard.Darl.Thinkbase.Meta;
using DarlCompiler.Ast;
using DarlCompiler.Interpreter;
using DarlCompiler.Parsing;
using System.Collections.Generic;
using System.Linq;

namespace Darl.Thinkbase.Meta
{
    [Language("Darl.Meta", "1.0", "Darl Meta language, Copyright(c) ThinkBase LLC, 2022")]
    public class DarlMetaGrammar : InterpretedLanguageGrammar
    {

        public List<DarlResult> results { get; set; }

        public GraphObject currentNode { get; set; }

        public IGraphModel currentModel { get; set; }

        public KnowledgeState state { get; set; }

        public List<DarlTime>? now { get; set; }

        public IMetaStructureHandler structure { get; set; }

        public DarlMetaGrammar() : base(true)
        {
            #region comments
            var singleLineComment = new CommentTerminal("SingleLineComment", "//", "\r", "\n", "\u2085", "\u2028", "\u2029");
            var delimitedComment = new CommentTerminal("DelimitedComment", "/*", "*/");
            var insertionComment = new CommentTerminal("DelimitedComment", "%%", "%%");
            NonGrammarTerminals.Add(singleLineComment);
            NonGrammarTerminals.Add(delimitedComment);
            NonGrammarTerminals.Add(insertionComment);
            #endregion

            #region keywords
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
            var NETWORK = Keyword("network", "A variable that is tied to a node in the knowledge graph.");
            var TEXTUAL = Keyword("textual", "A variable containing text that is not in distinct categories");
            var TEMPORAL = Keyword("temporal", "A variable containing a time value");
            var CONSTANT = Keyword("constant", "A numeric real valued constant.");
            var STRING = Keyword("string", "A string used for annotation or text processing.");
            var DURATION = Keyword("duration", "A time offset used for temporal processing.");
            var EXISTENCE = Keyword("existence", "the period of existence of the related node or attribute.");
            var AND = Keyword("and", "The logical 'and' of the operands either side, implemented as the minimum of their degrees of truth.");
            var OR = Keyword("or", "The logical 'or' of the operands either side, implemented as the maximum of their degrees of truth.");
            var NOT = Keyword("not", "The logical inverse of the operand after, implemented as 1 - the degree of truth.");
            var ANY = Keyword("any", "Combines all KG inputs of type matching the operands in an or structure.");
            var ALL = Keyword("all", "Combines all KG inputs of type matching the operands in an and structure.");
            var EXISTS = Keyword("exists");
            var ABSENT = Keyword("absent");
            var PRESENT = Keyword("present");
            var OTHERWISE = Keyword("otherwise", "Sets a rule that fires for the governing output if no other rules have fired.");
            var MINUSINFINITY = Keyword("-Infinity", "Represents negative infinity. Use to create sets unbounded on the lower side.");
            var PLUSINFINITY = Keyword("Infinity", "Represents positive infinity. Use to create sets unbounded on the upper side.");
            var MINTIME = Keyword("mintime", "Represents the start of representable time. Use to create sets unbounded on the lower side.");
            var MAXTIME = Keyword("maxtime", "Represents the end of representable time. Use to create sets unbounded on the upper side.");
            var NOW = Keyword("now", "Represents the current time and date.");
            var MULTIPLE = Keyword("multiple", "indicates the associated ruleset may return single or multiple results");
            var SHORTMINUSINFINITY = Keyword("-∞", "Represents negative infinity. Use to create sets unbounded on the lower side.");
            var SHORTPLUSINFINITY = Keyword("∞", "Represents positive infinity. Use to create sets unbounded on the upper side.");
            var CATEGORYOF = Keyword("categoryof", "Converts a categorical input or outputs value to the matched categorical output where category names match");
            var SUM = Keyword("sum", "Sums together a set of numeric values.");
            var PRODUCT = Keyword("product", "Multiplies together a set of numeric values.");
            var SIGMOID = Keyword("sigmoid", "Applies a Sigmoid logistic function to the supplied numeric value, and converts this to a degree of truth.");
            var DOCUMENT = Keyword("document", "Creates a textual output given a textual input and a set of parameters.");
            var NORMPROB = Keyword("normprob", "Applies a Gaussian probability density function to the value.");
            var ROUND = Keyword("round", "Rounds the first value to the limit of the second.");
            var MAXIMUM = Keyword("maximum", "The maximum of a set of numeric values");
            var MINIMUM = Keyword("minimum", "The minimum of a set of numeric values");
            var FUZZYTUPLE = Keyword("fuzzytuple", "A set of numeric values in ascending order that delineate a fuzzy set or interval.");
            var BEFORE = Keyword("before", "A comparison of two fuzzy times where the former wholly precedes the latter.");
            var PRECEDING = Keyword("preceding", "A comparison of two fuzzy times where the former precedes but joins with the latter.");
            var OVERLAPPING = Keyword("overlapping", "A comparison of two fuzzy times where the former and the latter are both current for a period");
            var DURING = Keyword("during", "A comparison of two fuzzy times where the former is completely contained within the latter");
            var STARTING = Keyword("starting", "A comparison of two fuzzy times where the former and the latter start at the same time.");
            var FINISHING = Keyword("finishing", "A comparison of two fuzzy times where the former and the latter end at the same time.");
            var AFTER = Keyword("after", "A comparison of two fuzzy times where the former wholly follows the latter.");
            var STORE = Keyword("store", "A store for associations that can be read and written to based on an index");
            var TIMERANGE = Keyword("timerange", "A set of temporal values in sequential order that delineate a fuzzy set or interval.");
            var MATCH = Keyword("match", "Applies a boolean returning regular expression, set by the string value, or sequence to the associated text input.");
            var RANDOMTEXT = Keyword("randomtext", "Randomly selects one of the input texts for output");
            var COUNT = Keyword("count", "Counts all KG inputs of type matching the operands.");
            var SEEK = Keyword("seek", "Seeks a goal within the network.");
            var ATTRIBUTE = Keyword("attribute", "Returns the value of an attribute. ");
            var DURATIONOF = Keyword("durationof", "Returns the duration of the existence of an object or attribute. ");
            var AGE = Keyword("age", "Returns the duration of the existence of an object or attribute relative to the current simulation time. ");
            var ATTRIBUTES = Keyword("attributes", "Returns the values of an attribute over a set of connected objects. ");
            var FOR = Keyword("for", "Adds a lifetime to an output value ");
            var SINGLE = Keyword("single", "Returns the first value of an attribute over a set of connected objects.");
            var LINEAGE = Keyword("lineage", "contains an address in an ontology");
            var DISCOVER = Keyword("discover", "Elucidates possibilities within a network given a state");
            #endregion

            #region identifiers

            var textual_input = new DarlMetaIdentifier("textual_input");
            var numeric_input = new DarlMetaIdentifier("numeric_input");
            var categorical_input = new DarlMetaIdentifier("categorical_input");
            var temporal_input = new DarlMetaIdentifier("temporal_input");
            var numeric_output = new DarlMetaIdentifier("numeric_output");
            var categorical_output = new DarlMetaIdentifier("categorical_output");
            var textual_output = new DarlMetaIdentifier("textual_output");
            var temporal_output = new DarlMetaIdentifier("temporal_output");
            var category = new DarlMetaIdentifier("category");
            var set = new DarlMetaIdentifier("set");
            var network_output = new DarlMetaIdentifier("network_output");
            var string_constant = new DarlMetaIdentifier("string_constant");
            var temporal_constant = new DarlMetaIdentifier("temporal_constant");
            var sequence_constant = new DarlMetaIdentifier("sequence_constant");
            var duration_constant = new DarlMetaIdentifier("duration_constant");
            var store_io = new DarlMetaIdentifier("store_io");
            var lineage_constant = new DarlMetaIdentifier("lineage_constant");

            #endregion

            #region literals

            var numberLiteral = new NumberLiteral("numberLiteral", NumberOptions.AllowSign);
            var stringLiteral = TerminalFactory.CreateCSharpString("stringLiteral");
            var categoryLiteral = new StringLiteral("categoryLiteral", "\"", StringOptions.None, typeof(CategoryLiteralNode));
            var durationLiteral = new FreeTextLiteral("durationLiteral", ";");
            var lineageLiteral = new StringLiteral("lineageLiteral", "\"", StringOptions.None, typeof(LineageLiteral));
            var nodeIdLiteral = new StringLiteral("nodeIdLiteral", "\"", StringOptions.None, typeof(NodeIdLiteral));

            #endregion

            #region NonTerminal Declarations
            // definitions
            var inputdefinition = new NonTerminal("inputdefinition", typeof(InputDefinitionNode));
            var outputdefinition = new NonTerminal("outputdefinition", typeof(OutputAsInputDefinitionNode));
            var constantdefinition = new NonTerminal("constantdefinition", typeof(ConstantDefinitionNode));
            var durationdefinition = new NonTerminal("durationdefinition", typeof(DurationDefinitionNode));
            var stringdefinition = new NonTerminal("stringdefinition", typeof(StringDefinitionNode));
            var lineagedefinition = new NonTerminal("lineagedefinition", typeof(LineageDefinitionNode));
            var set_definition = new NonTerminal("set_definition", typeof(SetDefinitionNode));
            var set_definitions = new NonTerminal("set_definitions", typeof(ExpressionListNode));
            var category_definitions = new NonTerminal("category_definitions", typeof(ExpressionListNode));
            var category_option = new NonTerminal("category_option");
            var set_option = new NonTerminal("set_option");
            var lineage_option = new NonTerminal("lineage_option");
            var network_source_option = new NonTerminal("network_source_option");
            var network_components = new NonTerminal("network_components", typeof(NetworkComponentNode));
            var rule = new NonTerminal("rule", typeof(RuleNode));
            var top_logical_op = new NonTerminal("top_logical_op");
            var program_root = new NonTerminal("program_root", typeof(MetaRootNode));
            var confidence = new NonTerminal("confidence", typeof(ConfidenceNode));
            var otherwise = new NonTerminal("otherwise", typeof(OtherwiseNode));
            var statement = new NonTerminal("statement");
            var statement_content = new NonTerminal("statement_content");
            var storedefinition = new NonTerminal("storedefinition", typeof(StoreDefinitionNode));
            var store = new NonTerminal("store", typeof(StoreNode));
            var store_rhs = new NonTerminal("store_rhs");
            var attribute_rhs = new NonTerminal("attribute_rhs");
            var confidence_choice = new NonTerminal("confidence_choice");
            var categoryChoice = new NonTerminal("categorychoice");
            var lifetime = new NonTerminal("lifetime", typeof(LifetimeNode));
            var lifetime_choice = new NonTerminal("lifetime_choice");
            var decoration = new NonTerminal("decoration", typeof(DecorationNode));
            var temporal_rvalue = new NonTerminal("temporal_rvalue");
            var network_rvalue = new NonTerminal("network_rvalue");
            var numeric_rvalue = new NonTerminal("numeric_rvalue");
            var textual_rvalue = new NonTerminal("textual_rvalue");
            var text_attribute = new NonTerminal("text_attribute");
            var arith_expression_list = new NonTerminal("arith_expression_list", typeof(ExpressionListNode));
            var arith_expression = new NonTerminal("arith_expression");
            var anything = new NonTerminal("anything", typeof(AnythingNode));
            var logical_expression = new NonTerminal("logical_expression");
            var logicalbrackets = new NonTerminal("logicalbrackets");
            var is_expression = new NonTerminal("is_expression", typeof(IsNode));
            var absent = new NonTerminal("absent", typeof(AbsentNode));
            var present = new NonTerminal("present", typeof(PresentNode));
            var comparitives = new NonTerminal("comparitives");
            var tempcomparitives = new NonTerminal("tempcomparitives");
            var match = new NonTerminal("match", typeof(MatchNode));
            var numberLiteral_list = new NonTerminal("numberLiteral_list", typeof(ExpressionListNode));
            var categoryof = new NonTerminal("categoryof", typeof(CategoryOfNode));
            var setValue = new NonTerminal("setvalue");
            var parameter_list = new NonTerminal("parameter_list");
            var brackets = new NonTerminal("brackets");
            var subsequence_choice = new NonTerminal("subsequence_choice");
            var textmatchchoice = new NonTerminal("textmatchchoice");
            var textsourcechoice = new NonTerminal("textsourcechoice");
            var anyio = new NonTerminal("anyio");
            var rvalue_choice = new NonTerminal("rvalue_choice");
            var temporal_expression = new NonTerminal("temporal_expression");
            var categoricalio = new NonTerminal("categoricalio");
            var numeric_constant = new DarlMetaIdentifier("numeric_constant");
            var anyio_list = new NonTerminal("anyio_list", typeof(AnyIOListNode));
            var now = new NonTerminal("now", typeof(NowNode));
            var mintime = new NonTerminal("mintime", typeof(MinTimeNode));
            var maxtime = new NonTerminal("maxtime", typeof(MaxTimeNode));
            var timerange = new NonTerminal("timerange", typeof(TimeRangeNode));
            var text_expression_list = new NonTerminal("text_expression_list", typeof(ExpressionListNode));
            var lineageList = new NonTerminal("lineageList", typeof(LineageListNode));
            var attribute = new NonTerminal("attribute", typeof(AttributeNode));
            var exists = new NonTerminal("exists", typeof(ExistsNode));
            var durationof = new NonTerminal("durationof", typeof(DurationOfNode));
            var existence = new NonTerminal("existence", typeof(ExistenceNode));
            var age = new NonTerminal("age", typeof(AgeNode));
            var attributes = new NonTerminal("attributes", typeof(AttributesNode));
            var single = new NonTerminal("single", typeof(SingleNode));
            var node_aggregation = new NonTerminal("node_aggregation");
            var aggregate_rhs = new NonTerminal("aggregate_rhs");
            var duration_expression = new NonTerminal("duration_expression");
            var lineage_choice = new NonTerminal("lineage_choice");


            this.Root = program_root;

            //operators
            var and = new NonTerminal("and", typeof(AndNode));
            var or = new NonTerminal("or", typeof(OrNode));
            var not = new NonTerminal("not", typeof(NotNode));
            var any = new NonTerminal("any", typeof(AnyNode));
            var all = new NonTerminal("all", typeof(AllNode));
            var count = new NonTerminal("count", typeof(CountNode));
            var seek = new NonTerminal("seek", typeof(SeekNode));
            var discover = new NonTerminal("discover", typeof(DiscoverNode));
            var eq = new NonTerminal("equal", typeof(EqualNode));
            var neq = new NonTerminal("not_equal", typeof(NotEqualNode));
            var greater = new NonTerminal("greater", typeof(GreaterNode));
            var lesser = new NonTerminal("lesser", typeof(LesserNode));
            var gteq = new NonTerminal("greater_equal", typeof(GreaterEqualNode));
            var lseq = new NonTerminal("lesser_equal", typeof(LesserEqualNode));
            var plus = new NonTerminal("plus", typeof(PlusNode));
            var minus = new NonTerminal("minus", typeof(MinusNode));
            var power = new NonTerminal("power", typeof(PowerNode));
            var modulus = new NonTerminal("modulus", typeof(ModulusNode));
            var multiply = new NonTerminal("multiply", typeof(MultiplyNode));
            var divide = new NonTerminal("divide", typeof(DivideNode));
            var normprob = new NonTerminal("normprob", typeof(NormProbNode));
            var sigmoid = new NonTerminal("sigmoid", typeof(SigmoidNode));
            var sum = new NonTerminal("sum", typeof(SumNode));
            var fuzzytuple = new NonTerminal("fuzzytuple", typeof(FuzzytupleNode));
            var minimum = new NonTerminal("minimum", typeof(MinimumNode));
            var maximum = new NonTerminal("maximum", typeof(MaximumNode));
            var round = new NonTerminal("round", typeof(RoundNode));
            var product = new NonTerminal("product", typeof(ProductNode));
            var temporal_plus = new NonTerminal("temporal_plus", typeof(TemporalPlusNode));
            var temporal_minus = new NonTerminal("temporal_minus", typeof(TemporalMinusNode));
            var before = new NonTerminal("before", typeof(BeforeNode));
            var preceding = new NonTerminal("preceding", typeof(PrecedesNode));
            var overlapping = new NonTerminal("overlapping", typeof(OverlapsNode));
            var during = new NonTerminal("during", typeof(DuringNode));
            var starting = new NonTerminal("starting", typeof(StartsNode));
            var finishing = new NonTerminal("finishing", typeof(FinishesNode));
            var after = new NonTerminal("after", typeof(AfterNode));
            var tempeq = new NonTerminal("temp_equal", typeof(TempEqualNode));
            var tempneq = new NonTerminal("temp_not_equal", typeof(TempNotEqualNode));
            var document = new NonTerminal("document", typeof(DocumentNode));
            var randomtext = new NonTerminal("randomtext", typeof(RandomTextNode));

            #endregion

            #region NonTerminal Rules

            program_root.Rule = MakeStarRule(program_root, null, statement);
            program_root.ErrorRule = SyntaxError + ToTerm("{}");

            statement.Rule = statement_content + ToTerm(";");

            statement.ErrorRule = SyntaxError + ToTerm(";");

            statement_content.Rule = rule | inputdefinition | outputdefinition | constantdefinition | stringdefinition | durationdefinition | storedefinition | otherwise | lineagedefinition;

            confidence.Rule = CONFIDENCE + numberLiteral;

            confidence_choice.Rule = Empty | confidence;

            lifetime.Rule = FOR + duration_expression;

            duration_expression.Rule = duration_constant | durationLiteral | age | durationof;

            lifetime_choice.Rule = Empty | lifetime;

            decoration.Rule = lifetime_choice + confidence_choice;

            rule.Rule = (IF + top_logical_op + THEN + categorical_output + WILL + BE + categoryChoice + decoration) |
                (IF + top_logical_op + THEN + numeric_output + WILL + BE + numeric_rvalue + decoration) |
                (IF + top_logical_op + THEN + textual_output + WILL + BE + textual_rvalue + decoration) |
                (IF + top_logical_op + THEN + temporal_output + WILL + BE + temporal_rvalue + decoration) |
                (IF + top_logical_op + THEN + network_output + WILL + BE + network_rvalue + decoration) |
                (IF + top_logical_op + THEN + store + WILL + BE + textual_rvalue + decoration) |
                (IF + top_logical_op + THEN + store + WILL + BE + arith_expression + decoration) |
                (IF + top_logical_op + THEN + store + WILL + BE + categorical_input + decoration);

            otherwise.Rule = (OTHERWISE + IF + top_logical_op + THEN + categorical_output + WILL + BE + categoryChoice + decoration) |
                (OTHERWISE + IF + top_logical_op + THEN + numeric_output + WILL + BE + numeric_rvalue + decoration) |
                (OTHERWISE + IF + top_logical_op + THEN + textual_output + WILL + BE + textual_rvalue + decoration) |
                (OTHERWISE + IF + top_logical_op + THEN + temporal_output + WILL + BE + temporal_rvalue + decoration) |
                (OTHERWISE + IF + top_logical_op + THEN + store + WILL + BE + textual_rvalue + decoration) |
                (OTHERWISE + IF + top_logical_op + THEN + store + WILL + BE + arith_expression + decoration) |
                (OTHERWISE + IF + top_logical_op + THEN + store + WILL + BE + categorical_input + decoration);

            top_logical_op.Rule = anything | logical_expression;

            logical_expression.Rule = and | or | not | is_expression | logicalbrackets | any | all | exists;

            logicalbrackets.Rule = "(" + logical_expression + ")";

            and.Rule = logical_expression + PreferShiftHere() + AND + logical_expression;

            or.Rule = logical_expression + PreferShiftHere() + OR + logical_expression;

            not.Rule = NOT + logical_expression;

            anything.Rule = ANYTHING;

            exists.Rule = EXISTS + "(" + lineage_option + ")";

            existence.Rule = EXISTENCE + "(" + network_source_option + ")";

            durationof.Rule = DURATIONOF + "(" + lineage_option + ")";

            age.Rule = AGE + "(" + temporal_expression + ")";

            lineage_choice.Rule = lineage_constant | lineageLiteral;

            lineageList.Rule = MakePlusRule(lineageList, ToTerm(","), lineage_choice);

            any.Rule = ANY + "(" + lineageList + ")";

            all.Rule = ALL + "(" + lineageList + ")";

            count.Rule = COUNT + "(" + lineageList + ")";

            attribute.Rule = ATTRIBUTE + attribute_rhs;

            attribute_rhs.Rule = "(" + lineage_choice + ")";

            attributes.Rule = ATTRIBUTES + "(" + lineageList + ")";

            single.Rule = SINGLE + "(" + lineageList + ")";

            absent.Rule = ABSENT;

            present.Rule = PRESENT;

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
                            (numeric_output + IS + absent) |
                            (textual_input + IS + absent) |
                            (numeric_input + IS + present) |
                            (temporal_input + IS + absent) |
                            (temporal_output + IS + absent) |
                            (temporal_input + IS + present) |
                            (temporal_output + IS + present) |
                            (categorical_output + IS + present) |
                            (categorical_input + IS + present) |
                            (numeric_output + IS + present) |
                            (textual_input + IS + present) |
                            (textual_output + IS + present) |
                            (store + IS + comparitives) |
                            (store + IS + match) |
                            (store + IS + absent) |
                            (store + IS + present) |
                            (count + IS + comparitives) |
                            (durationof + IS + comparitives) |
                            (age + IS + comparitives)
                            );

            inputdefinition.Rule = (INPUT + NUMERIC + numeric_input + set_option + network_source_option) |
                       (INPUT + CATEGORICAL + categorical_input + category_option + network_source_option) |
                       (INPUT + TEXTUAL + textual_input + network_source_option) |
                       (INPUT + TEMPORAL + temporal_input + network_source_option);

            outputdefinition.Rule = (OUTPUT + NUMERIC + numeric_output + set_option + lineage_option) |
                       (OUTPUT + CATEGORICAL + categorical_output + category_option + lineage_option) |
                       (OUTPUT + TEXTUAL + textual_output + lineage_option) |
                       (OUTPUT + TEMPORAL + temporal_output + lineage_option) |
                       (OUTPUT + NETWORK + network_output + nodeIdLiteral + lineage_choice);

            set_option.Rule = Empty | "{" + set_definitions + "}";

            set_definitions.Rule = MakePlusRule(set_definitions, ToTerm(","), set_definition);

            set_definition.Rule = "{" + set + "," + numberLiteral_list + "}";

            category_definitions.Rule = MakePlusRule(category_definitions, ToTerm(","), categoryChoice);

            categoryChoice.Rule = categoryLiteral | category | categoryof | attribute | single /* | Empty*/; //ANE 07/02/21 - removed EMPTY

            numberLiteral_list.Rule = MakePlusRule(numberLiteral_list, ToTerm(","), setValue);

            setValue.Rule = numberLiteral | MINUSINFINITY | PLUSINFINITY | SHORTMINUSINFINITY | SHORTPLUSINFINITY;

            categoryof.Rule = CATEGORYOF + "(" + categoricalio + ")";

            categoricalio.Rule = categorical_input | categorical_output;

            comparitives.Rule = eq | neq | greater | lesser | gteq | lseq;

            lineage_option.Rule = Empty | lineage_choice;

            network_source_option.Rule = Empty | network_components;

            network_components.Rule = nodeIdLiteral + lineage_choice;

            eq.Rule = "=" + arith_expression;
            neq.Rule = "!=" + arith_expression;
            greater.Rule = ">" + arith_expression;
            lesser.Rule = "<" + arith_expression;
            gteq.Rule = ">=" + arith_expression;
            lseq.Rule = "<=" + arith_expression;

            arith_expression.Rule = plus | minus | multiply | divide | modulus | power | numeric_constant | numeric_input | numeric_output | sum | product | fuzzytuple | minimum | maximum | sigmoid | normprob | round | brackets | numberLiteral | count | attribute | durationof | duration_constant | single | age;

            tempeq.Rule = "=" + temporal_expression;
            tempneq.Rule = "!=" + temporal_expression;
            before.Rule = BEFORE + temporal_expression;
            preceding.Rule = PRECEDING + temporal_expression;
            overlapping.Rule = OVERLAPPING + temporal_expression;
            during.Rule = DURING + temporal_expression;
            finishing.Rule = FINISHING + temporal_expression;
            starting.Rule = STARTING + temporal_expression;
            after.Rule = AFTER + temporal_expression;

            aggregate_rhs.Rule = node_aggregation | parameter_list;

            sum.Rule = SUM + aggregate_rhs;

            product.Rule = PRODUCT + aggregate_rhs;

            fuzzytuple.Rule = FUZZYTUPLE + parameter_list;

            minimum.Rule = MINIMUM + aggregate_rhs;

            maximum.Rule = MAXIMUM + aggregate_rhs;

            sigmoid.Rule = SIGMOID + "(" + arith_expression + ")";

            normprob.Rule = NORMPROB + "(" + arith_expression + ")";

            round.Rule = ROUND + "(" + arith_expression + "," + arith_expression + ")";

            anyio.Rule = numeric_input | numeric_output | categorical_input | categorical_output | textual_input | textual_output | temporal_input | temporal_output | network_output;

            anyio_list.Rule = MakePlusRule(anyio_list, ToTerm(","), anyio);

            categoricalio.Rule = categorical_input | categorical_output;

            arith_expression_list.Rule = MakePlusRule(arith_expression_list, ToTerm(","), arith_expression);

            numberLiteral_list.Rule = MakePlusRule(numberLiteral_list, ToTerm(","), setValue);

            setValue.Rule = numberLiteral | MINUSINFINITY | PLUSINFINITY | SHORTMINUSINFINITY | SHORTPLUSINFINITY;

            textmatchchoice.Rule = string_constant | sequence_constant | stringLiteral | attribute;

            node_aggregation.Rule = "(" + attributes + ")";

            brackets.Rule = "(" + arith_expression + ")";

            plus.Rule = arith_expression + PreferShiftHere() + "+" + arith_expression;

            minus.Rule = arith_expression + PreferShiftHere() + "-" + arith_expression;

            multiply.Rule = arith_expression + PreferShiftHere() + "*" + arith_expression;

            divide.Rule = arith_expression + PreferShiftHere() + "/" + arith_expression;

            modulus.Rule = arith_expression + PreferShiftHere() + "%" + arith_expression;

            power.Rule = arith_expression + PreferShiftHere() + "^" + arith_expression;

            temporal_plus.Rule = temporal_expression + PreferShiftHere() + "+" + duration_expression | duration_expression + PreferShiftHere() + "+" + temporal_expression;

            temporal_minus.Rule = temporal_expression + PreferShiftHere() + "-" + duration_expression;

            temporal_expression.Rule = temporal_plus | temporal_minus | temporal_input | temporal_output | now | mintime | maxtime | attribute | single | existence;

            parameter_list.Rule = "(" + arith_expression_list + ")";

            storedefinition.Rule = STORE + store_io;

            parameter_list.Rule = "(" + arith_expression_list + ")";

            store_rhs.Rule = "[" + text_expression_list + "]";

            store.Rule = store_io + store_rhs;

            tempcomparitives.Rule = before | preceding | overlapping | during | starting | finishing | after | tempeq | tempneq;

            now.Rule = NOW;

            mintime.Rule = MINTIME;

            maxtime.Rule = MAXTIME;

            timerange.Rule = TIMERANGE + parameter_list;

            match.Rule = MATCH + textmatchchoice;

            text_expression_list.Rule = MakePlusRule(text_expression_list, ToTerm(","), textsourcechoice);

            textsourcechoice.Rule = stringLiteral | textual_input | string_constant | textual_output;

            numeric_rvalue.Rule = set | arith_expression | store;

            temporal_rvalue.Rule = temporal_expression | store;
            text_attribute.Rule = attribute | single;

            textual_rvalue.Rule = textual_input | document | string_constant | randomtext | stringLiteral | store | text_attribute;

            network_rvalue.Rule = seek | discover;

            seek.Rule = SEEK + "(" + lineageList + ")";

            discover.Rule = DISCOVER + "(" + lineageList + ")";

            document.Rule = DOCUMENT + "(" + textsourcechoice + "," + "{" + anyio_list + "}" + ")";

            category_option.Rule = Empty | "{" + category_definitions + "}";

            randomtext.Rule = RANDOMTEXT + "(" + text_expression_list + ")";

            constantdefinition.Rule = CONSTANT + numeric_constant + numberLiteral;

            durationdefinition.Rule = DURATION + duration_constant + durationLiteral;

            stringdefinition.Rule = STRING + string_constant + stringLiteral;

            storedefinition.Rule = STORE + store_io;

            lineagedefinition.Rule = LINEAGE + lineage_constant + lineageLiteral;



            #endregion

            RegisterOperators(1, "or");
            RegisterOperators(2, "and");
            RegisterOperators(3, "+", "-");
            RegisterOperators(3, "not");
            RegisterOperators(4, "*", "/", "%");
            RegisterOperators(4, IS);
            RegisterOperators(5, "^");
            RegisterOperators(6, ".");

            MarkPunctuation("{", "}", "(", ")", "[", "]", ",", ";", ".", "if", "then", "will", "be", "confidence", "input", "output", "numeric", "categorical", "arity", "presence", "string", "constant", "or", "and", "not", "is", "*", "/", "-", "+", "%", "^", ">", "<", "<=", ">=", "anything", "textual", "maximum", "minimum", "sum", "product", "fuzzytuple", "sigmoid", "normprob", "round", "pattern", "absent", "present", "delay", "sequence", "match", "document", "randomtext", "store", "temporal", "categoryof", "duration", "now", "mintime", "maxtime", "after", "before", "preceding", "overlapping", "during", "finishing", "starting", "all", "any", "count", "seek", "network", "attribute", "attributes", "exists", "durationof", "for", "single", "lineage", "discover", "existence", "age");
            RegisterBracePair("(", ")");
            RegisterBracePair("{", "}");
            RegisterBracePair("[", "]");
            MarkTransient(top_logical_op, statement_content, category_option, logical_expression, set_option,
                numeric_rvalue, arith_expression, statement, comparitives, tempcomparitives, parameter_list, brackets, setValue,
                categoryChoice, subsequence_choice,
                textmatchchoice, anyio, textual_rvalue, textsourcechoice, logicalbrackets, rvalue_choice, store_rhs, temporal_rvalue,
                temporal_expression, categoricalio, lineage_option, network_rvalue, attribute_rhs, confidence_choice, text_attribute,
                node_aggregation, aggregate_rhs, lifetime_choice, duration_expression, lineage_choice, network_source_option);

            LanguageFlags = LanguageFlags.CreateAst;

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


        public override void BuildAst(LanguageData language, ParseTree parseTree, Dictionary<string, DarlLanguage.Processing.ILocalStore> stores)
        {
            if (!LanguageFlags.IsSet(LanguageFlags.CreateAst))
                return;
            var astContext = new AstContext(language);
            astContext.DefaultIdentifierNodeType = typeof(DarlMetaIdentifierNode);
            astContext.DefaultLiteralNodeType = typeof(DarlMetaNumberLiteralNode);
            var astBuilder = new AstBuilder(astContext);
            astBuilder.BuildAst(parseTree);
        }

        public DarlResult? ResultByName(string name)
        {
            if (results.Any(a => a.name == name))
            {
                return results.First(a => a.name == name);
            }
            return null;
        }

        public DarlResult NetWorkResults(string objectId, string lineage)
        {
            var res = currentModel.FindDataAttribute(objectId, lineage, state);
            if(res != null)
            {
                var r =  DarlVarExtensions.Convert(res);
                results.Add(r);
                return r;
            }
            return new DarlResult(0.0, true);
        }

        /// <summary>
        /// Ensure the object id and lineage reference an existing attribute within an existing object, linked to with the given connection lineage
        /// </summary>
        /// <param name="id">id or externalId</param>
        /// <param name="lineage">Attribute lineage</param>
        /// <param name="connectionLineage">Lineage of the connection, or any if null</param>
        /// <returns>true,the object and the att if a valid connection exists and false otherwise.</returns>
        public (bool, GraphObject?, GraphAttribute?) TraceNetworkConnection(string id, string lineage, string? connectionLineage)
        {
            var answerLineage = "noun:01,4,05,21,19";
            GraphObject? obj = null;
            if (!currentModel.vertices.ContainsKey(id))
            {
                obj = currentModel.vertices.Values.FirstOrDefault(a => a.externalId == id);
                if (obj == null)
                    return (false,null,null);
                id = obj.id ?? "";
            }
            else
            {
                obj = currentModel.vertices[id];
            }
            //check link exists between currentNode and found node
            if(!currentNode.In.Any(a => a.startId == id && a.endId == currentNode.id) && !currentNode.Out.Any(a => a.startId == currentNode.id && a.endId == id))
                return (false,obj,null);
            if (obj.properties != null)
            {
                var oatt = obj.properties.Where(a => a.lineage != null && a.lineage.StartsWith(lineage)).FirstOrDefault();
                if (oatt != null)
                    return (true,obj,oatt);
                //what if the attribute is up in the virtual world? 
                //consider implicit setup where sub nodes determine the structure. 
                if(/*lineage == answerLineage &&*/ obj.Out.Any())
                    return (true,obj,null);
            }
            //finally in the virtual realm - generic values
            if (obj.lineage == null)
                return (false, null, null);
            var virtNode = currentModel.virtualVertices[obj.lineage];
            var list1 = new List<GraphObject> { virtNode };
            currentModel.FollowHypernymy(virtNode, list1);
            GraphAttribute? data = null;
            bool found = false;
            foreach (var l in list1)
            {
                if (l.properties != null)
                {
                    foreach (var p in l.properties)
                    {
                        if (p.lineage != null && p.lineage.StartsWith(lineage))
                        {
                            data = p;
                            found = true;
                            break;
                        }
                    }
                    foreach (var p in l.properties) //if no match look for rules - could be there
                    {
                        if (p.type == GraphAttribute.DataType.ruleset )
                        {
                            data = p;
                            found = true;
                            break;
                        }
                    }
                }
                if (found)
                    break;
            }
            return found ? (true,obj,data) : (false,obj,null);
        }
    }

}
