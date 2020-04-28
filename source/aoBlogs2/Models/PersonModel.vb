﻿
Option Explicit On
Option Strict On

Imports System
Imports System.Collections.Generic
Imports System.Text
Imports Contensive.BaseClasses

Namespace Models
    Public Class PersonModel
        Inherits DbModel
        Implements ICloneable
        '
        '====================================================================================================
        '-- const
        Public Const contentName As String = "People"      '<------ set content name
        Public Const contentTableName As String = "ccMembers"   '<------ set to tablename for the primary content (used for cache names)
        Private Shadows Const contentDataSource As String = "default"             '<------ set to datasource if not default
        '
        '====================================================================================================
        ' -- instance properties

        Public Property Address As String
        Public Property Address2 As String
        Public Property Admin As Boolean
        Public Property AdminMenuModeID As Integer
        Public Property AllowBulkEmail As Boolean
        Public Property AllowToolsPanel As Boolean
        'Public Property authorInfoLink As String
        Public Property AutoLogin As Boolean
        Public Property BillAddress As String
        Public Property BillAddress2 As String
        Public Property BillCity As String
        Public Property BillCompany As String
        Public Property BillCountry As String
        Public Property BillEmail As String
        Public Property BillFax As String
        Public Property BillName As String
        Public Property BillPhone As String
        Public Property BillState As String
        Public Property BillZip As String
        Public Property BirthdayDay As Integer
        Public Property BirthdayMonth As Integer
        Public Property BirthdayYear As Integer
        Public Property City As String
        Public Property Company As String
        Public Property Country As String
        Public Property CreatedByVisit As Boolean
        Public Property DateExpires As Date
        Public Property Developer As Boolean
        Public Property Email As String
        Public Property ExcludeFromAnalytics As Boolean
        Public Property Fax As String
        Public Property FirstName As String
        Public Property ImageFilename As String
        Public Property LanguageID As Integer
        Public Property LastName As String
        Public Property LastVisit As Date
        Public Property nickName As String
        Public Property NotesFilename As String
        Public Property OrganizationID As Integer
        Public Property Password As String
        Public Property Phone As String
        Public Property ResumeFilename As String
        Public Property ShipAddress As String
        Public Property ShipAddress2 As String
        Public Property ShipCity As String
        Public Property ShipCompany As String
        Public Property ShipCountry As String
        Public Property ShipName As String
        Public Property ShipPhone As String
        Public Property ShipState As String
        Public Property ShipZip As String
        Public Property State As String
        'Public Property StyleFilename As String
        Public Property ThumbnailFilename As String
        Public Property Title As String
        Public Property Username As String
        Public Property Visits As Integer
        Public Property Zip As String


        '
        '====================================================================================================
        Public Overloads Shared Function add(cp As CPBaseClass) As PersonModel
            Return add(Of PersonModel)(cp)
        End Function
        '
        '====================================================================================================
        Public Overloads Shared Function create(cp As CPBaseClass, recordId As Integer) As PersonModel
            Return create(Of PersonModel)(cp, recordId)
        End Function
        '
        '====================================================================================================
        Public Overloads Shared Function create(cp As CPBaseClass, recordGuid As String) As PersonModel
            Return create(Of PersonModel)(cp, recordGuid)
        End Function
        '
        '====================================================================================================
        Public Overloads Shared Function createByName(cp As CPBaseClass, recordName As String) As PersonModel
            Return createByName(Of PersonModel)(cp, recordName)
        End Function
        '
        '====================================================================================================
        Public Overloads Sub save(cp As CPBaseClass)
            MyBase.save(Of PersonModel)(cp)
        End Sub
        '
        '====================================================================================================
        Public Overloads Shared Sub delete(cp As CPBaseClass, recordId As Integer)
            delete(Of PersonModel)(cp, recordId)
        End Sub
        '
        '====================================================================================================
        Public Overloads Shared Sub delete(cp As CPBaseClass, ccGuid As String)
            delete(Of PersonModel)(cp, ccGuid)
        End Sub
        '
        '====================================================================================================
        Public Overloads Shared Function createList(cp As CPBaseClass, sqlCriteria As String, Optional sqlOrderBy As String = "id") As List(Of PersonModel)
            Return createList(Of PersonModel)(cp, sqlCriteria, sqlOrderBy)
        End Function
        '
        '====================================================================================================
        Public Overloads Shared Function getRecordName(cp As CPBaseClass, recordId As Integer) As String
            Return DbModel.getRecordName(Of PersonModel)(cp, recordId)
        End Function
        '
        '====================================================================================================
        Public Overloads Shared Function getRecordName(cp As CPBaseClass, ccGuid As String) As String
            Return DbModel.getRecordName(Of PersonModel)(cp, ccGuid)
        End Function
        '
        '====================================================================================================
        Public Overloads Shared Function getRecordId(cp As CPBaseClass, ccGuid As String) As Integer
            Return DbModel.getRecordId(Of PersonModel)(cp, ccGuid)
        End Function
        '
        '====================================================================================================
        Public Overloads Shared Function getCount(cp As CPBaseClass, sqlCriteria As String) As Integer
            Return DbModel.getCount(Of PersonModel)(cp, sqlCriteria)
        End Function
        '
        '====================================================================================================
        Public Overloads Function getUploadPath(fieldName As String) As String
            Return MyBase.getUploadPath(Of PersonModel)(fieldName)
        End Function
        '
        '====================================================================================================
        '
        Public Function Clone(cp As CPBaseClass) As PersonModel
            Dim result As PersonModel = DirectCast(Me.Clone(), PersonModel)
            result.id = cp.Content.AddRecord(contentName)
            result.ccguid = cp.Utils.CreateGuid()
            result.save(of BlogModel)(cp)
            Return result
        End Function
        '
        '====================================================================================================
        '
        Public Function Clone() As Object Implements ICloneable.Clone
            Return Me.MemberwiseClone()
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' true if the person can edit the blog
        ''' </summary>
        ''' <param name="cp"></param>
        ''' <param name="blog"></param>
        ''' <returns></returns>
        Public Function isBlogEditor(cp As CPBaseClass, blog As BlogModel) As Boolean
            Dim blogAuthorsGroupId As Integer = cp.Group.GetId("Blog Authors")
            If blogAuthorsGroupId = 0 Then
                cp.Group.Add("Blog Authors")
                blogAuthorsGroupId = cp.Group.GetId("Blog Authors")
            End If
            Return cp.User.IsAuthenticated And ((id.Equals(blog.OwnerMemberID)) OrElse (Admin) OrElse (cp.User.IsInGroupList(blogAuthorsGroupId.ToString(), id)))
        End Function
    End Class
End Namespace