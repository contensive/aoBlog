Imports Contensive.BaseClasses

Namespace Models.View
    Public Class RequestModel
        '
        '====================================================================================================
        '
        Private ReadOnly cp As CPBaseClass
        '
        '
        Public Sub New(cp As CPBaseClass)
            Me.cp = cp

        End Sub
        '
        '
        Public ReadOnly Property page As Integer
            Get
                Return cp.Doc.GetInteger(rnPageNumber)
            End Get
        End Property
        '
        '
        Public ReadOnly Property instanceGuid As String
            Get
                instanceGuid = cp.Doc.GetText("instanceId")
                If (String.IsNullOrWhiteSpace(instanceGuid)) Then instanceGuid = "BlogWithoutinstanceId-PageId-" & cp.Doc.PageId
                Return instanceGuid
            End Get
        End Property
        '
        '
        Public ReadOnly Property srcViewId As Integer
            Get
                Return cp.Doc.GetInteger(rnFormID)
            End Get
        End Property
        '
        '
        Public Property dstViewId As Integer
        '
        '====================================================================================================
        ''' <summary>
        ''' category Id
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property categoryId As Integer
            Get
                If (_categoryId Is Nothing) Then
                    If (Not String.IsNullOrEmpty(cp.Doc.GetText(RequestNameBlogCategoryIDSet))) Then
                        _categoryId = cp.Doc.GetInteger(RequestNameBlogCategoryIDSet)
                    Else
                        _categoryId = cp.Doc.GetInteger(RequestNameBlogCategoryID)
                    End If
                End If
                Return Convert.ToInt32(_categoryId)
            End Get
        End Property
        Private _categoryId As Integer? = Nothing
        '
        '====================================================================================================
        ''' <summary>
        ''' The Blog Entry Id requested
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property blogEntryId As Integer
            Get
                If (_blogEntryId Is Nothing) Then
                    _blogEntryId = cp.Doc.GetInteger(RequestNameBlogEntryID)
                End If
                Return Convert.ToInt32(_blogEntryId)
            End Get
        End Property
        Private _blogEntryId As Integer? = Nothing
        '
        '====================================================================================================
        '
        Public ReadOnly Property ButtonValue As String
            Get
                If (_ButtonValue Is Nothing) Then
                    _ButtonValue = cp.Doc.GetText("button")
                End If
                Return Convert.ToString(_ButtonValue)
            End Get
        End Property
        Private _ButtonValue As String = Nothing
        '
        '====================================================================================================
        '
        Public ReadOnly Property FormID As Integer
            Get
                If (_FormID Is Nothing) Then
                    _FormID = cp.Doc.GetInteger(rnFormID)
                End If
                Return Convert.ToInt32(_FormID)
            End Get
        End Property
        Private _FormID As Integer? = Nothing

        '
        '====================================================================================================
        '
        Public ReadOnly Property SourceFormID As Integer
            Get
                If (_SourceFormID Is Nothing) Then
                    _SourceFormID = cp.Doc.GetInteger(RequestNameSourceFormID)
                End If
                Return Convert.ToInt32(_SourceFormID)
            End Get
        End Property
        Private _SourceFormID As Integer? = Nothing
        '
        '====================================================================================================
        '
        Public ReadOnly Property KeywordList As String
            Get
                If (_KeywordList Is Nothing) Then
                    _KeywordList = cp.Doc.GetText(RequestNameKeywordList)
                End If
                Return Convert.ToString(_KeywordList)
            End Get
        End Property
        Private _KeywordList As String = Nothing
        '
        '====================================================================================================
        '
        Public ReadOnly Property DateSearchText As String
            Get
                If (_DateSearchText Is Nothing) Then
                    _DateSearchText = cp.Doc.GetText(RequestNameDateSearch)
                End If
                Return Convert.ToString(_DateSearchText)
            End Get
        End Property
        Private _DateSearchText As String = Nothing
        '
        '====================================================================================================
        '
        Public ReadOnly Property ArchiveMonth As Integer
            Get
                If (_ArchiveMonth Is Nothing) Then
                    _ArchiveMonth = cp.Doc.GetInteger(RequestNameArchiveMonth)
                End If
                Return Convert.ToInt32(_ArchiveMonth)
            End Get
        End Property
        Private _ArchiveMonth As Integer? = Nothing
        '
        '====================================================================================================
        '
        Public ReadOnly Property ArchiveYear As Integer
            Get
                If (_ArchiveYear Is Nothing) Then
                    _ArchiveYear = cp.Doc.GetInteger(RequestNameArchiveYear)
                End If
                Return Convert.ToInt32(_ArchiveYear)
            End Get
        End Property
        Private _ArchiveYear As Integer? = Nothing
        '
        '====================================================================================================
        '
        Public ReadOnly Property EntryID As Integer
            Get
                If (_EntryID Is Nothing) Then
                    _EntryID = cp.Doc.GetInteger(RequestNameBlogEntryID)
                End If
                Return Convert.ToInt32(_EntryID)
            End Get
        End Property
        Private _EntryID As Integer? = Nothing
        '
        '====================================================================================================
        '
        Public ReadOnly Property BlogEntryName As String
            Get
                If (_BlogEntryName Is Nothing) Then
                    _BlogEntryName = cp.Doc.GetText(RequestNameBlogEntryName)
                End If
                Return Convert.ToString(_BlogEntryName)
            End Get
        End Property
        Private _BlogEntryName As String = Nothing
        '
        '====================================================================================================
        '
        Public ReadOnly Property BlogEntryCopy As String
            Get
                If (_BlogEntryCopy Is Nothing) Then
                    _BlogEntryCopy = cp.Doc.GetText(RequestNameBlogEntryCopy)
                End If
                Return Convert.ToString(_BlogEntryCopy)
            End Get
        End Property
        Private _BlogEntryCopy As String = Nothing
        '
        '====================================================================================================
        '
        Public ReadOnly Property BlogEntryTagList As String
            Get
                If (_BlogEntryTagList Is Nothing) Then
                    _BlogEntryTagList = cp.Doc.GetText(RequestNameBlogEntryTagList)
                End If
                Return Convert.ToString(_BlogEntryTagList)
            End Get
        End Property
        Private _BlogEntryTagList As String = Nothing
        '
        '====================================================================================================
        '
        Public ReadOnly Property BlogEntryCategoryId As Integer
            Get
                If (_BlogEntryCategoryId Is Nothing) Then
                    _BlogEntryCategoryId = cp.Doc.GetInteger(RequestNameBlogEntryCategoryID)
                End If
                Return Convert.ToInt32(_BlogEntryCategoryId)
            End Get
        End Property
        Private _BlogEntryCategoryId As Integer? = Nothing
    End Class
End Namespace
