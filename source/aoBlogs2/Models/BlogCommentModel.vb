
Imports Contensive.Models.Db

Namespace Models
    Public Class BlogCommentModel
        Inherits DbBaseModel
        '
        '====================================================================================================
        ''' <summary>
        '''table definition
        '''</summary>
        Public Shared ReadOnly Property tableMetadata As DbBaseTableMetadataModel = New DbBaseTableMetadataModel("Blog Comments", "ccBlogComments", "default", False)
        '
        '====================================================================================================
        '
        ' -- instance properties
        'instancePropertiesGoHere
        Public Property AllowComments As Boolean
        Public Property Anonymous As Boolean
        Public Property Approved As Boolean
        Public Property articlePrimaryImagePositionId As Integer
        Public Property AuthorMemberID As Integer
        Public Property blogCategoryID As Integer
        Public Property BlogID As Integer
        ' Public Property Copy As String
        Public Property EntryID As Integer
        Public Property FormKey As String
        Public Property imageDisplayTypeId As Integer
        Public Property primaryImagePositionId As Integer
        Public Property Viewings As Integer
        Public Property CopyText As String

    End Class
End Namespace
