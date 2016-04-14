# ReportMasterTwo
Fixed Column and CSV Format Textual Reporting Tool. ReportMasterTwo was originally created to replace a deprecated reporting tool called ReportMaster used by Sage MAS 90/200 which allowed precise fixed column layouts of text 

Here is an example of a RMT file that specifies a report runnable by ReportMasterTwo

    #Config Start#  
    Connection-String:Dsn=SOTAMAS90  
    #Format Start#
    [LITERAL:E] 01/001 {0:#}  
    [LITERAL:9999999999] 01/002 {0:0000000000}  
    [PARAM:YEAR] 01/012 {0:0000}  
    [PARAM:QUARTER] 01/016 {0:0}  
    [COUNT(PR_22PerpetChkHistoryHeader.EmployeeNumber)] 01/041 {0:0000000}  
    [100*SUM(PR_22PerpetChkHistoryHeader.GrossWagesThisCheck)] 01/048 {0:0000000000000}  
    #Sql Start#  
    SELECT 100*SUM(PR_22PerpetChkHistoryHeader.GrossWagesThisCheck), COUNT(PR_22PerpetChkHistoryHeader.EmployeeNumber) FROM  
    PR_22PerpetChkHistoryHeader WHERE {fn QUARTER(PR_22PerpetChkHistoryHeader.CheckDate)} = $QUARTER AND {fn YEAR(PR_22PerpetChkHistoryHeader.CheckDate)} = $YEAR  
    #Report End#  

Config Start opens the configuration

Connection-String: sets the connection string.

Format Start is used in Fixed Column reports, to start the field definitions (CSV Start in CSV reports)

Lines in the format start section should follow the syntax:

    [declaration] LN/COL {0,align:fmt} 

where declaration can start with LITERAL, PARAM, = or a field clause in the following select statement. Field clauses are matched up with the terms of the select statement and must be identical with a field in the select.

LN is the line number the field will be on. This allows for multiple line per record layouts. COL is the column number the line will be on.

align is the text alignment of the format. Negative values indicate left aligned text with the given width, padded on the right if the value is less than the given length. Positive values indicate right aligned text.

The fmt is a .NET formatting code. The whole block from {0,align:fmt} matches the .net formatting code.

The fields used here are SQL calculations, set the field name to match the text of the calculation, and you can use any calculations supported in your variant of SQL.. You can also do ReportMasterTwo calculations by starting the field declaration with =. Only +, -, * and / are available for ReportMasterTwo calculations.

Here is a ReportMasterTwo style calculation, which also shows alignment:

    [=SO_InvoiceHeader.NonTaxableAmt + SO_InvoiceHeader.TaxableAmt - SO_InvoiceHeader.FreightAmt - SO_InvoiceHeader.SalesTaxAmt - SO_InvoiceHeader.DiscountAmt] 02/142 {0, 13:#########.00}

Sql Start begins the SQL query section. This must be a SELECT query, not any other command.

params defined in the field section can be referred to by #name.

Report End demarcates the end of the report document.

You can also define a detail subreport in a RMT file.

After Format Start but before Sql Start include a line:

    #Detail Start ON [SO_InvoiceHeader.InvoiceNo] 04/001 {0}

After detail start you then have a new Format Start, Sql Start and Report End and then fall back to the master report's Sql Start and Report End.

The field referred to in ON {field} is accessible as $D. 

As in 

    ("SO_InvoiceDetail"."InvoiceNo" = '$D') 

Further details and example will be available in the Wiki soon.
