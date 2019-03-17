
Option Explicit On
Option Strict On

Imports System
Imports System.Collections.Generic
Imports System.Text
Imports Contensive.BaseClasses

Namespace Models
    Public Class EmailModel
        Inherits DbModel
        '
        '====================================================================================================
        '-- const
        Public Const contentName As String = "Email"      '<------ set content name
        Public Const contentTableName As String = "ccEmail"   '<------ set to tablename for the primary content (used for cache names)
        Private Shadows Const contentDataSource As String = "default"             '<------ set to datasource if not default
        '
        '====================================================================================================
        ' -- instance properties

        Public Property AddLinkEID As Boolean
        Public Property AllowSpamFooter As Boolean
        Public Property BlockSiteStyles As Boolean
        Public Property ConditionExpireDate As Date
        Public Property ConditionID As Integer
        Public Property ConditionPeriod As Integer
        Public Property CopyFilename As String
        Public Property EmailTemplateID As Integer
        Public Property EmailWizardID As Integer
        Public Property FromAddress As String
        Public Property InlineStyles As String
        Public Property LastSendTestDate As Date
        Public Property ScheduleDate As Date
        Public Property Sent As Boolean
        Public Property StylesFilename As String
        Public Property Subject As String
        Public Property Submitted As Boolean
        Public Property TestMemberID As Integer
        Public Property ToAll As Boolean


    End Class
End Namespace
