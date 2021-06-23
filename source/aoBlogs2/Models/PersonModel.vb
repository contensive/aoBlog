
Imports Contensive.BaseClasses

Namespace Models
    Public Class PersonModel
        Inherits Contensive.Models.Db.PersonModel
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