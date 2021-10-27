Option Explicit On
Option Strict On

Imports System.Linq
Imports Contensive.Addons.Blog.Models
Imports Contensive.BaseClasses

Namespace Controllers
    Public NotInheritable Class LinkAliasController
        '
        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="cp"></param>
        ''' <param name="defaultLink"></param>
        ''' <returns></returns>
        Public Shared Function getLinkAlias(cp As CPBaseClass, defaultLink As String) As String
            '
            Dim result As String = ""
            Try
                result = defaultLink
                If cp.Site.GetBoolean("allowLinkAlias", True) Then
                    Dim Link As String = defaultLink
                    '
                    Dim pageQs() As String = Split(LCase(Link), "?")
                    If UBound(pageQs) > 0 Then
                        Dim nameValues() As String = Split(pageQs(1), "&")
                        Dim cnt As Integer = UBound(nameValues) + 1
                        If UBound(nameValues) < 0 Then
                        Else
                            Dim qs As String = ""
                            Dim Ptr As Integer
                            Dim pageId As Integer
                            For Ptr = 0 To cnt - 1
                                Dim NameValue As String = nameValues(Ptr)
                                If pageId = 0 Then
                                    If Mid(NameValue, 1, 4) = "bid=" Then
                                        pageId = cp.Utils.EncodeInteger(Mid(NameValue, 5))
                                        NameValue = ""
                                    End If
                                End If
                                If NameValue <> "" Then
                                    qs = qs & "&" & NameValue
                                End If
                            Next
                            If pageId <> 0 Then
                                If Len(qs) > 1 Then
                                    qs = Mid(qs, 2)
                                End If
                                result = cp.Content.GetLinkAliasByPageID(pageId, qs, defaultLink)
                            End If
                        End If
                    End If
                End If
                getLinkAlias = result
                '
            Catch ex As Exception
                cp.Site.ErrorReport(ex)
            End Try
            Return result
        End Function
        '
        ''' <summary>
        ''' Add a blog entry to link alias
        ''' </summary>
        ''' <param name="cp"></param>
        ''' <param name="blogPostname"></param>
        ''' <param name="pageId"></param>
        ''' <param name="blogEntryId"></param>
        Public Shared Sub addLinkAlias(cp As CPBaseClass, blogPostname As String, pageId As Integer, blogEntryId As Integer)
            Dim qs As String = getLinkAliasQueryString(cp, pageId, blogEntryId)
            cp.Site.AddLinkAlias(blogPostname, cp.Doc.PageId, qs)
        End Sub
        '
        ''' <summary>
        ''' get the linkalias querystring for this blogid
        ''' </summary>
        ''' <param name="cp"></param>
        ''' <param name="pageId"></param>
        ''' <param name="blogEntryId"></param>
        ''' <returns></returns>
        Public Shared Function getLinkAliasQueryString(cp As CPBaseClass, pageId As Integer, blogEntryId As Integer) As String
            Dim qs As String = cp.Utils.ModifyQueryString("", RequestNameBlogEntryID, CStr(blogEntryId))
            Return cp.Utils.ModifyQueryString(qs, rnFormID, FormBlogPostDetails.ToString())
        End Function
        '
        ''' <summary>
        ''' delete linkalias for this blog entry
        ''' </summary>
        ''' <param name="cp"></param>
        ''' <param name="pageId"></param>
        ''' <param name="blogEntryId"></param>
        Public Shared Sub deleteLinkAlias(cp As CPBaseClass, pageId As Integer, blogEntryId As Integer)
            Dim linkAliasQS As String = getLinkAliasQueryString(cp, pageId, blogEntryId)
            For Each linkAlias In DbModel.createList(Of LinkAliasesModel)(cp, "(QueryStringSuffix=" & cp.Db.EncodeSQLText(linkAliasQS) & ")")
                DbModel.delete(Of LinkAliasesModel)(cp, linkAlias.id)
            Next
        End Sub

    End Class
End Namespace

