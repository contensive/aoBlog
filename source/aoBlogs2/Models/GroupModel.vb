
Imports Contensive.BaseClasses
Imports Contensive.Models.Db

Namespace Models
    Public Class GroupModel
        Inherits Contensive.Models.Db.PersonModel
        '
        '====================================================================================================
        '
        Public Shared Function GetBlockingGroups(cp As CPBaseClass, BlogCategoryID As Integer) As List(Of GroupModel)
            '
            Dim result As New List(Of GroupModel)
            '           
            For Each BlogCategoryGroupRule In DbBaseModel.createList(Of BlogCategoryGroupRulesModel)(cp, "BlogCategoryID=" & BlogCategoryID)
                result.Add(DbBaseModel.create(Of GroupModel)(cp, BlogCategoryGroupRule.GroupID))
            Next
            Return result
        End Function


    End Class
End Namespace