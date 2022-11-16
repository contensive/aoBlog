
Namespace Models
    Public Class BlogEntryCTARuleModel
        Inherits DbModel
        '
        '====================================================================================================
        '-- const
        Public Const contentName As String = "Blog Entry CTA Rules"
        Public Const contentTableName As String = "ccBlogEntryCTARules"
        Private Shadows Const contentDataSource As String = "default"
        '
        '====================================================================================================
        ' -- instance properties
        Public Property blogentryid As Integer
        Public Property calltoactionid As Integer
    End Class
End Namespace
