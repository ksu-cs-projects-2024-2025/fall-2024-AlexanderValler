Public Class HomeController
    Inherits System.Web.Mvc.Controller

    Function Index() As ActionResult
        ViewData("Title") = "BlockOut | Home"

        Return View()
    End Function
End Class
