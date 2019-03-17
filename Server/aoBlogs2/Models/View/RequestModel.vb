

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
                End If
                Return _instanceId
            End Get
        End Property
        Private _instanceId As String = Nothing
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
        ' todo - convert these to ondemand properties
        '
        Public ReadOnly Property ButtonValue As String
            Get
                Return cp.Doc.GetText("button")
            End Get
        End Property
        '
        Public ReadOnly Property FormID As Integer
            Get
                Return cp.Doc.GetInteger(RequestNameFormID)
            End Get
        End Property
        '
        Public ReadOnly Property SourceFormID As Integer
            Get
                Return cp.Doc.GetInteger(RequestNameSourceFormID)
            End Get
        End Property
        '
        Public ReadOnly Property KeywordList As String
            Get
                Return cp.Doc.GetText(RequestNameKeywordList)
            End Get
        End Property
        '
        Public ReadOnly Property DateSearchText As String
            Get
                Return cp.Doc.GetText(RequestNameDateSearch)
            End Get
        End Property
        '
        Public ReadOnly Property ArchiveMonth As Integer
            Get
                Return cp.Doc.GetInteger(RequestNameArchiveMonth)
            End Get
        End Property
        '
        Public ReadOnly Property ArchiveYear As Integer
            Get
                Return cp.Doc.GetInteger(RequestNameArchiveYear)
            End Get
        End Property
        Public ReadOnly Property EntryID As Integer
            Get
                Return cp.Doc.GetInteger(RequestNameBlogEntryID)
            End Get
        End Property
        '
        Public ReadOnly Property BlogEntryName As String
            Get
                Return cp.Doc.GetText(RequestNameBlogEntryName)
            End Get
        End Property
        '
        Public ReadOnly Property BlogEntryCopy As String
            Get
                Return cp.Doc.GetText(RequestNameBlogEntryCopy)
            End Get
        End Property
        '
        Public ReadOnly Property BlogEntryTagList As String
            Get
                Return cp.Doc.GetText(RequestNameBlogEntryTagList)
            End Get
        End Property
        '
        Public ReadOnly Property BlogEntryCategoryId As Integer
            Get
                Return cp.Doc.GetInteger(RequestNameBlogEntryCategoryID)
            End Get
        End Property
        '
        '
        '
        Public Sub New(cp As CPBaseClass)
            Me.cp = cp
        End Sub
    End Class
End Namespace
