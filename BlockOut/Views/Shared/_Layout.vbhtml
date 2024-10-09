<!DOCTYPE html>
<html>
<head>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width" />
    <title>@ViewBag.Title</title>
    @Styles.Render("~/Content/css")
    @Scripts.Render("~/bundles/modernizr")

    <style>
        /* Custom CSS for the navbar */
        .navbar-custom {
            background: linear-gradient(to bottom, #101010, #5e5e5e); /* Light gray to dark gray */
            padding-top: 60px; /* Remove extra padding to keep it consistent */
            height: 80px; /* Adjust the navbar height to fit the image */
            display: flex;
            align-items: center; /* Vertically align items */
            border: none; /* Remove any borders */
        }

        /* Adjust the navbar height to accommodate the image */
        .navbar-brand img {
            height: 100px; /* Adjust the image size to fit */
        }

        /* Add padding to the body so the content is not behind the navbar */
        body {
            padding-top: 100px; /* Adjust the top padding to avoid overlap */
        }

        /* Optional: Adjust the navbar toggle button for mobile view */
        .navbar-toggle {
            margin-top: 15px; /* Align the toggle button vertically if using */
        }
    </style>

</head>
<body>
    <div class="navbar navbar-inverse navbar-fixed-top navbar-custom navbar-brand img">
        <div class="container">
            <div class="navbar-header">
                <button type="button" class="navbar-toggle" data-toggle="collapse" data-target=".navbar-collapse">
                    <span class="icon-bar"></span>
                    <span class="icon-bar"></span>
                    <span class="icon-bar"></span>
                </button>
                <!-- Manually creating the anchor tag with an image -->
                <a href="@Url.Action("Index", "Home", New With {.area = ""})" class="navbar-brand">
                    <img src="BlockoutLogo_Name.PNG"/>
                </a>
            </div>
            <div class="navbar-collapse collapse">
                <ul class="nav navbar-nav">
                    <li>@Html.ActionLink("Home", "Index", "Home", New With {.area = ""}, Nothing)</li>
                    <li>@Html.ActionLink("About", "Index", "Help", New With {.area = ""}, Nothing)</li>
                </ul>
                <!-- Login floated to the top right corner -->
                <ul class="nav navbar-nav navbar-right">
                    <li>@Html.ActionLink("Sign in", "Index", "Login", New With {.area = ""}, Nothing)</li>
                </ul>
            </div>
        </div>
    </div>
    <div class="container body-content">
        @RenderBody()
        <hr />
        <footer>
            <p>&copy; @DateTime.Now.Year - My ASP.NET Application</p>
        </footer>
    </div>

    @Scripts.Render("~/bundles/jquery")
    @Scripts.Render("~/bundles/bootstrap")
    @RenderSection("scripts", required:=False)
</body>
</html>
