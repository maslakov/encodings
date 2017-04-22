# HTML Parser coding challenge
## Task definition
A program to determine the encoding of Web pages.

There is a text file with a list of page addresses.
File format: each line contains one address, encoding utf-8.

For each page, you need to get the encoding identifier from the "meta" tag from the page text.
If there are several such tags in the text of the page, you need to get all of them.
If there is no such tag in the source of the page, the encoding is determined as the default encoding.
If the encoding is specified with an error, it must be replaced with the correct one, using the lookup table.

For example: the incorrect identifier of the iso-8859 encoding must be replaced by the correct identifier iso-8859-1.

The lookup table is contained in a text file.

The number of substitutions is not less than 10.

The file format is free.

The results of the program should be placed in a text file.

File format: each line contains one pair of address-encoding, delimiter - tabulation, encoding utf-8.

The process of the program should be reflected in the log file.

Errors in processing should not lead to an emergency stop of the program.

## Solution
in the class *UrlProcessor* 2 solutions are provided:
- based on .NET regex parsing
- based on custom parser (*SimpleTagSearcher*) backed by deterministic automata

Performance difference is more than 10x in favor of custom parser.
