Imports Contensive.Addons.Blog.Controllers
Imports Contensive.BaseClasses

Public Class LatestPostWidgetViewModel
    Public Class LatestNewsWidgetViewModel
        Inherits DesignBlockViewBaseModel

        Public Property mainCell As LatestNewsViewModel
        Public Property secondCell As LatestNewsViewModel
        Public Property thirdCell As LatestNewsViewModel
        Public Property lastCell As LatestNewsViewModel
        Public Property latestNewsAddTag As String

        Public Shared Function create(ByVal cp As CPBaseClass, ByVal settings As DbLatestNewsWidgetsModel) As LatestNewsWidgetViewModel
            Dim hint As Integer = 0

            Try
                Dim result = create(Of LatestNewsWidgetViewModel)(cp, settings)
                Dim latestNews As List(Of DbLatestNewsModel) = DbLatestNewsModel.createList(Of DbLatestNewsModel)(cp, "", "newsDate desc", 4, 1)
                Dim newsPageId As Integer = cp.Site.GetInteger("latest news widget pageid")
                result.mainCell = New LatestNewsViewModel()
                result.secondCell = New LatestNewsViewModel()
                result.thirdCell = New LatestNewsViewModel()
                result.lastCell = New LatestNewsViewModel()

                If latestNews.Count >= 1 Then
                    LinkAliasController.addLinkAlias(cp, latestNews(0).name, newsPageId, latestNews(0).id)
                    result.mainCell.continueURL = LinkAliasController.getLinkAlias(cp, cp.Http.WebAddressProtocolDomain + latestNews(0).name.Trim().Replace(" ", "-"), newsPageId)
                    result.mainCell.description = LatestNewsController.limitString(cp, cp.Utils.ConvertHTML2Text(latestNews(0).copy), 175)
                    result.mainCell.newsDate = latestNews(0).newsDate.ToString("MMMM dd, yyyy")
                    result.mainCell.header = latestNews(0).header
                    result.mainCell.newsImage = cp.Http.CdnFilePathPrefix + cp.Image.GetBestFitWebP(latestNews(0).newsImage, 550, 452)
                    result.mainCell.LatestNewsElementEditTag = cp.Content.GetEditLink("Latest News", latestNews(0).id, "Edit Latest News element")
                End If

                If latestNews.Count >= 2 Then
                    LinkAliasController.addLinkAlias(cp, latestNews(1).name, newsPageId, latestNews(1).id)
                    result.secondCell.continueURL = LinkAliasController.getLinkAlias(cp, cp.Http.WebAddressProtocolDomain + latestNews(1).name.Trim().Replace(" ", "-"), newsPageId)
                    result.secondCell.description = LatestNewsController.limitString(cp, cp.Utils.ConvertHTML2Text(latestNews(1).copy), 175)
                    result.secondCell.newsDate = latestNews(1).newsDate.ToString("MMMM dd, yyyy")
                    result.secondCell.header = latestNews(1).header
                    result.secondCell.newsImage = cp.Http.CdnFilePathPrefix + cp.Image.GetBestFitWebP(latestNews(1).newsImage, 191, 148)
                    result.secondCell.LatestNewsElementEditTag = cp.Content.GetEditLink("Latest News", latestNews(1).id, "Edit Latest News element")
                End If

                If latestNews.Count >= 3 Then
                    LinkAliasController.addLinkAlias(cp, latestNews(2).name, newsPageId, latestNews(2).id)
                    result.thirdCell.continueURL = LinkAliasController.getLinkAlias(cp, cp.Http.WebAddressProtocolDomain + latestNews(2).name.Trim().Replace(" ", "-"), newsPageId)
                    result.thirdCell.description = LatestNewsController.limitString(cp, cp.Utils.ConvertHTML2Text(latestNews(2).copy), 175)
                    result.thirdCell.newsDate = latestNews(2).newsDate.ToString("MMMM dd, yyyy")
                    result.thirdCell.header = latestNews(2).header
                    result.thirdCell.newsImage = cp.Http.CdnFilePathPrefix + cp.Image.GetBestFitWebP(latestNews(2).newsImage, 191, 148)
                    result.thirdCell.LatestNewsElementEditTag = cp.Content.GetEditLink("Latest News", latestNews(2).id, "Edit Latest News element")
                End If

                If latestNews.Count >= 4 Then
                    LinkAliasController.addLinkAlias(cp, latestNews(3).name, newsPageId, latestNews(3).id)
                    result.lastCell.continueURL = LinkAliasController.getLinkAlias(cp, cp.Http.WebAddressProtocolDomain + latestNews(3).name.Trim().Replace(" ", "-"), newsPageId)
                    result.lastCell.description = LatestNewsController.limitString(cp, cp.Utils.ConvertHTML2Text(latestNews(3).copy), 175)
                    result.lastCell.newsDate = latestNews(3).newsDate.ToString("MMMM dd, yyyy")
                    result.lastCell.header = latestNews(3).header
                    result.lastCell.newsImage = cp.Http.CdnFilePathPrefix + cp.Image.GetBestFitWebP(latestNews(3).newsImage, 191, 148)
                    result.lastCell.LatestNewsElementEditTag = cp.Content.GetEditLink("Latest News", latestNews(3).id, "Edit Latest News element")
                End If

                result.latestNewsAddTag = If(cp.User.IsEditing(""), cp.Content.GetAddLink("Latest News", $"latestNewsWidgetId={settings.id}"), "")
                Return result
            Catch ex As Exception
                cp.Site.ErrorReport("Error in LatestNewsWidgetViewModel create hint = : " & hint & " " & ex)
                Return New LatestNewsWidgetViewModel()
            End Try
        End Function
    End Class
End Class