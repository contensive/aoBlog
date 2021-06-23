
Imports Contensive.Models.Db

Namespace Models
    Public Class BlogCategoryGroupRulesModel
        Inherits DbBaseModel
        '
        '====================================================================================================
        ''' <summary>
        '''table definition
        '''</summary>
        Public Shared ReadOnly Property tableMetadata As DbBaseTableMetadataModel = New DbBaseTableMetadataModel("Blog Category Group Rules", "ccBlogCategoryGroupRules", "default", False)
        '
        '====================================================================================================
        ' -- instance properties
        '
        Public Property BlogCategoryID As Integer
        Public Property GroupID As Integer
        '
    End Class
End Namespace
