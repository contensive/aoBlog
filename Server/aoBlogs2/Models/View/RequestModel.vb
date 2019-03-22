

Option Explicit On
Option Strict On

Imports System
Imports System.Collections.Generic
Imports System.Text
Imports Contensive.BaseClasses

Namespace Models.View
    Public Class RequestModel
        Inherits DbModel
        '
        '====================================================================================================
        '
        Private cp As CPBaseClass
        ''' <summary>
        ''' string that represents the guid of the blog record to be displayed
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property instanceId As String
            Get
                If (_instanceId Is Nothing) Then
                    _instanceId = cp.Doc.GetText("instanceId")
                    If (String.IsNullOrWhiteSpace(_instanceId)) Then _instanceId = "BlogWithoutinstanceId-PageId-" & cp.Doc.PageId
                    cp.Utils.AppendLog("aoBlog, RequestModel, instanceId [" & _instanceId & "]")
                End If
                Return _instanceId
            End Get
        End Property
        Private _instanceId As String = Nothing
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
                    cp.Utils.AppendLog("aoBlog, RequestModel, categoryId [" & _categoryId & "]")
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
                    cp.Utils.AppendLog("aoBlog, RequestModel, _blogEntryId [" & _blogEntryId & "]")
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
                    cp.Utils.AppendLog("aoBlog, RequestModel, _ButtonValue [" & _ButtonValue & "]")
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
                    _FormID = cp.Doc.GetInteger(RequestNameFormID)
                    cp.Utils.AppendLog("aoBlog, RequestModel, _FormID [" & _FormID & "]")
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
                    cp.Utils.AppendLog("aoBlog, RequestModel, _SourceFormID [" & _SourceFormID & "]")
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
                    cp.Utils.AppendLog("aoBlog, RequestModel, _KeywordList [" & _KeywordList & "]")
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
                    cp.Utils.AppendLog("aoBlog, RequestModel, _DateSearchText [" & _DateSearchText & "]")
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
                    cp.Utils.AppendLog("aoBlog, RequestModel, _ArchiveMonth [" & _ArchiveMonth & "]")
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
                    cp.Utils.AppendLog("aoBlog, RequestModel, _ArchiveYear [" & _ArchiveYear & "]")
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
                    cp.Utils.AppendLog("aoBlog, RequestModel, _EntryID [" & _EntryID & "]")
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
                    cp.Utils.AppendLog("aoBlog, RequestModel, _BlogEntryName [" & _BlogEntryName & "]")
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
                    cp.Utils.AppendLog("aoBlog, RequestModel, _BlogEntryCopy [" & _BlogEntryCopy & "]")
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
                    cp.Utils.AppendLog("aoBlog, RequestModel, _BlogEntryTagList [" & _BlogEntryTagList & "]")
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
                    cp.Utils.AppendLog("aoBlog, RequestModel, _BlogEntryCategoryId [" & _BlogEntryCategoryId & "]")
                End If
                Return Convert.ToInt32(_BlogEntryCategoryId)
            End Get
        End Property
        Private _BlogEntryCategoryId As Integer? = Nothing
        '
        '====================================================================================================
        '
        Public Sub New(cp As CPBaseClass)
            Me.cp = cp
        End Sub
    End Class
End Namespace
