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

Server.Start();
// var a = new UtilsTest();
// a.TestCreateMockUsers();
// var addedUsers = WebApp.Utils.CreateMockUsers();
//new UtilsTest().TestCreateMockUsers(); // Anropa testmetoden
