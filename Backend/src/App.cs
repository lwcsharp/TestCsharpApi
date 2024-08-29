// Global settings
Globals = Obj(new
{
    debugOn = true,
    detailedAclDebug = false,
    aclOn = true, //false = avstängt
    isSpa = true,
    port = 3001,
    serverName = "Ironboy's Minimal API Server",
    frontendPath = FilePath("..", "Frontend"),
    sessionLifeTimeHours = 2
});
// Server.Start(); ////utkommentera: annars>ny commit>auto github actions
// Log(WebApp.Utils.CreateMockUsers());
// UtilsTest.TestCreateMockUsers();
// var addedUsers = WebApp.Utils.CreateMockUsers();
// UtilsTest.TestCreateMockUsers(); // Anropa testmetoden