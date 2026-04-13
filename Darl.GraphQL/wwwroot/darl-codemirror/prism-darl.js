/**
 * @module prism-darl.js
 */

﻿Prism.languages.darl = Prism.languages.extend('clike', {
	'keyword': /\b(?:if|anything|is|then|will|be|confidence|input|output|numeric|categorical|textual|constant|string|sum|product|sigmoid|normprob|round|match|and|or|not|maximum|minimum|fuzzytuple|exists|absent|present|sequence|supervised|document|randomtext|otherwise|store|duration|temporal|categoryof|timerange|before|preceding|overlapping|during|starting|finishing|after|now|dynamic|count|seek|attribute|attributes|for|single|durationof|lineage|any|all|for)\b/,
	'string': [
		{
			pattern: /@("|')(?:\1\1|\\[\s\S]|(?!\1)[^\\])*\1/,
			greedy: true
		},
		{
			pattern: /("|')(?:\\.|(?!\1)[^\\\r\n])*?\1/,
			greedy: true
		}
	],
	'number': /\b0x[\da-f]+\b|(?:\b\d+\.?\d*|\B\.\d+)f?/i
});

