
Imports Contensive.Models.Db

Namespace Models
    Public Class BlogCategorieModel
        Inherits DbBaseModel
        '
        '====================================================================================================
        ''' <summary>
        '''table definition
        '''</summary>
        Public Shared ReadOnly Property tableMetadata As DbBaseTableMetadataModel = New DbBaseTableMetadataModel("Blog Categories", "ccBlogCategories", "default", False)
        '
        '====================================================================================================
        Public Property userBlocking As Boolean

    End Class
End Namespace
