// Darl.Meta language support
return {
	defaultToken: '',
	tokenPostfix: '.darl',

	brackets: [
		{ open: '{', close: '}', token: 'delimiter.curly' },
		{ open: '[', close: ']', token: 'delimiter.square' },
		{ open: '(', close: ')', token: 'delimiter.parenthesis' }
	],

	keywords: [
		"if", "then", "will", "be", "confidence", "input", "output", "numeric",
		"categorical", "arity", "presence", "string", "constant", "or", "and",
		"not", "is", "anything", "textual", "maximum", "minimum", "sum", "product",
		"fuzzytuple", "sigmoid", "normprob", "round", "absent", "present", "delay",
		"sequence", "match", "document", "randomtext", "store", "temporal",
		"categoryof", "duration", "now", "mintime", "maxtime", "after", "before",
		"preceding", "overlapping", "during", "finishing", "starting", "all", "any",
		"count", "seek", "network", "attribute", "exists", "durationof"
	],

	parenFollows: [
		'categoryof', 'sum', 'product', 'fuzzytuple', 'minimum', 'maximum', 'sigmoid',
		'normprob', 'round', 'seek', 'document', 'randomtext'
	],

	operators: [
		'=', '|', '^', '<=', '>=', '<', '>',
		'+', '-', '*', '/', '%'
	],

	symbols: /[=><!~?:&|+\-*\/\^%]+/,

	// escape sequences
	escapes: /\\(?:[abfnrtv\\"']|x[0-9A-Fa-f]{1,4}|u[0-9A-Fa-f]{4}|U[0-9A-Fa-f]{8})/,

	// The main tokenizer for our languages
	tokenizer: {
		root: [

			// identifiers and keywords
			[/\@?[a-zA-Z_]\w*/, {
				cases: {
					'@keywords': { token: 'keyword.$0', next: '@qualified' },
					'@default': { token: 'identifier', next: '@qualified' }
				}
			}],

			// whitespace
			{ include: '@whitespace' },

			// delimiters and operators
			[/}/, {
				cases: {
					'$S2==interpolatedstring': { token: 'string.quote', next: '@pop' },
					'$S2==litinterpstring': { token: 'string.quote', next: '@pop' },
					'@default': '@brackets'
				}
			}],
			[/[{}()\[\]]/, '@brackets'],
			[/[<>](?!@symbols)/, '@brackets'],
			[/@symbols/, {
				cases: {
					'@operators': 'delimiter',
					'@default': ''
				}
			}],


			// numbers
			[/[0-9_]*\.[0-9_]+([eE][\-+]?\d+)?[fFdD]?/, 'number.float'],
			[/0[xX][0-9a-fA-F_]+/, 'number.hex'],
			[/0[bB][01_]+/, 'number.hex'], // binary: use same theme style as hex
			[/[0-9_]+/, 'number'],

			// delimiter: after number because of .\d floats
			[/[;,.]/, 'delimiter'],

			// strings
			[/"([^"\\]|\\.)*$/, 'string.invalid'],  // non-teminated string
			[/"/, { token: 'string.quote', next: '@string' }],
			[/\$\@"/, { token: 'string.quote', next: '@litinterpstring' }],
			[/\@"/, { token: 'string.quote', next: '@litstring' }],
			[/\$"/, { token: 'string.quote', next: '@interpolatedstring' }],

			// characters
			[/'[^\\']'/, 'string'],
			[/(')(@escapes)(')/, ['string', 'string.escape', 'string']],
			[/'/, 'string.invalid']
		],

		qualified: [
			[/[a-zA-Z_][\w]*/, {
				cases: {
					'@keywords': { token: 'keyword.$0' },
					'@default': 'identifier'
				}
			}],
			[/\./, 'delimiter'],
			['', '', '@pop'],
		],

		comment: [
			[/[^\/*]+/, 'comment'],
			// [/\/\*/,    'comment', '@push' ],    // no nested comments :-(
			['\\*/', 'comment', '@pop'],
			[/[\/*]/, 'comment']
		],

		string: [
			[/[^\\"]+/, 'string'],
			[/@escapes/, 'string.escape'],
			[/\\./, 'string.escape.invalid'],
			[/"/, { token: 'string.quote', next: '@pop' }]
		],

		litstring: [
			[/[^"]+/, 'string'],
			[/""/, 'string.escape'],
			[/"/, { token: 'string.quote', next: '@pop' }]
		],

		litinterpstring: [
			[/[^"{]+/, 'string'],
			[/""/, 'string.escape'],
			[/{{/, 'string.escape'],
			[/}}/, 'string.escape'],
			[/{/, { token: 'string.quote', next: 'root.litinterpstring' }],
			[/"/, { token: 'string.quote', next: '@pop' }]
		],

		interpolatedstring: [
			[/[^\\"{]+/, 'string'],
			[/@escapes/, 'string.escape'],
			[/\\./, 'string.escape.invalid'],
			[/{{/, 'string.escape'],
			[/}}/, 'string.escape'],
			[/{/, { token: 'string.quote', next: 'root.interpolatedstring' }],
			[/"/, { token: 'string.quote', next: '@pop' }]
		],

		whitespace: [
			[/^[ \t\v\f]*#((r)|(load))(?=\s)/, 'directive.csx'],
			[/^[ \t\v\f]*#\w.*$/, 'namespace.cpp'],
			[/[ \t\v\f\r\n]+/, ''],
			[/\/\*/, 'comment', '@comment'],
			[/\/\/.*$/, 'comment'],
		],
	},
};
