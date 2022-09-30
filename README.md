# Sharable-Spreadsheet-object

C# project - Introduction to operating systems course 2nd semester, 2nd year.

This project was done as part of the “Introduction to operating systems course”. the aim of the project is to implement an in-memory shared spreadsheet management object. The spreadsheet object supports several elementary operations. The spreadsheet object is used by many threads concurrently and hence needs to be designed to be thread-safe.


The motivation: This is core object of a spreadsheet that can be shared between multiple concurrent users like Google Docs and Google Sheets. 

The spreadsheet represent a table of n*m cells (n=rows, m=columns).

Each cell holds a string (C# string).

The spreadsheet starts at cell 0,0 (top, left).
