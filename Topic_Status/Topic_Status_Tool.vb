Imports System.IO

Module Topic_Status_Tool
    Public GUID As New ArrayList
    Public VersionNum As New ArrayList
    Public Resolution As New ArrayList
    Public pubName As New ArrayList
    Public langName As New ArrayList
    Private f_Properties As New Hashtable

    Sub Main(args As String())
        Try
            If args.Length <> 3 Then
                Console.WriteLine("You have provided wrong number of input arguments")
                Return
            ElseIf Not args(1).EndsWith(".csv") Then
                Console.WriteLine("You have not provided the csv file in the first argument")
            End If

            Dim usrDir As String = Environment.CurrentDirectory
            Dim toolPropertyFile As String = usrDir & "\tool.properities"
            If Not File.Exists(toolPropertyFile) Then
                Console.WriteLine("Missing property file in the folder:" & usrDir)
                Return
            End If
            Dim wholeProperty As String = My.Computer.FileSystem.ReadAllText(toolPropertyFile)
            Dim lineDataProperty() As String = Split(wholeProperty, vbNewLine)

            If f_Properties.Count = 0 Then
                For Each lineTextProperty As String In lineDataProperty
                    f_Properties.Add(lineTextProperty.Split("=")(0), lineTextProperty.Split("=")(1))
                Next
            End If

            Dim UserName As String = f_Properties.Item("USERNAME")
            Dim Password As String = f_Properties.Item("PASSWORD")
            Dim URL As String = f_Properties.Item("HOMEURL")

            Dim PubType As String = args(0)

            Dim filePath = args(1)
            Dim savePath = filePath.Substring(0, filePath.LastIndexOf("\"))

            Dim Log_Path = args(2) & "\log.txt"
            If Not File.Exists(Log_Path) Then
                File.Create(Log_Path).Dispose()
            End If

            Dim wholeFile As String
            Dim lineData() As String
            Dim fieldData() As String
            Dim excelFileName = filePath.Substring(filePath.LastIndexOf("\") + 1)
            Dim Context As String = ""
            Dim testobj As New IshObjs(UserName, Password, URL)
            testobj.ISHAppObj.Login("InfoShareAuthor", UserName, Password, Context)
            Dim test As New IshPubOutput(UserName, Password, URL)
            Dim outputFileLang As String = ""
            Dim i As Integer
            If Not String.IsNullOrEmpty(filePath) And Not String.IsNullOrEmpty(savePath) Then
                wholeFile = My.Computer.FileSystem.ReadAllText(filePath)
                lineData = Split(wholeFile, vbNewLine)
                For Each lineOfText As String In lineData
                    fieldData = lineOfText.Split(",")
                    For i = 0 To fieldData.Length - 1
                        Select Case i
                            Case 0
                                GUID.Add(fieldData(0))
                            Case 1
                                VersionNum.Add(fieldData(1))
                            Case 2
                                Resolution.Add(fieldData(2))
                            Case 3
                                langName.Add(fieldData(3))
                            Case 4
                                pubName.Add(fieldData(4))
                        End Select
                    Next
                Next lineOfText
            Else
                Console.WriteLine("The file path is empty.")
                Return
            End If

            GUID.RemoveAt(GUID.Count - 1)

            Dim oExcel As Object
            Dim oBook As Object
            Dim oSheet As Object
            'Start a new workbook in Excel.
            oExcel = CreateObject("Excel.Application")
            oBook = oExcel.Workbooks.Add
            'Add data to cells of the first worksheet in the new workbook.
            oSheet = oBook.Worksheets(1)
            oSheet.Cells(1, 1).Value = "Pub GUID"
            oSheet.Cells(1, 2).Value = "Pub Name"
            oSheet.Cells(1, 3).Value = "Pub Version"
            oSheet.Cells(1, 4).Value = "Resolution"
            oSheet.Cells(1, 5).Value = "Topic GUID"
            oSheet.Cells(1, 6).Value = "Topic Type"
            oSheet.Cells(1, 7).Value = "Topic Name"
            oSheet.Cells(1, 8).Value = "Topic Version"
            oSheet.Cells(1, 9).Value = "Topic Status"
            oSheet.Cells(1, 10).Value = "Language"
            oSheet.Cells(1, 11).Value = "EN Release Date"
            oSheet.Cells(1, 12).Value = "Author"
            oSheet.Cells(1, 13).Value = "Enable L10N"
            oSheet.Cells(1, 14).Value = "In Translation Date"
            oSheet.Cells(1, 15).Value = "Translated Date"
            oSheet.Cells(1, 16).Value = "Comments"
            Dim excelRow As Integer = 2

            Dim j As Integer
            For j = 0 To GUID.Count - 1
                Dim id As String = GUID(j).ToString()
                Dim version As String = VersionNum(j).ToString()
                Dim reso As String = Resolution(j).ToString()
                Dim lang As String = langName(j).ToString()
                Dim name As String = pubName(j).ToString()
                outputFileLang = lang
                test.GetStatus(id, name, version, reso, lang, PubType, savePath, Log_Path, excelRow, oSheet)
            Next
            oSheet.Range("K1", "K" & excelRow).HorizontalAlignment = 3
            oSheet.Range("N1", "N" & excelRow).HorizontalAlignment = 3
            oSheet.Range("O1", "O" & excelRow).HorizontalAlignment = 3S
            oSheet.Range("A1:P1").EntireColumn.AutoFit()

            Dim time As DateTime = DateTime.Now
            Dim format As String = "MMddyyyyHHmm"
            Dim stringTime As String = time.ToString(format)
            excelFileName = excelFileName.Replace(".csv", "_TopicStatus_" & stringTime & ".xlsx")
            oBook.SaveAs(savePath & "\" & excelFileName)
            oSheet = Nothing
            oBook = Nothing
            oExcel.Quit()
            oExcel = Nothing
            GC.Collect()
            Console.WriteLine("The Task is Done")

        Catch ex As Exception
            Console.WriteLine(ex.Message)
        End Try
        
    End Sub

End Module
