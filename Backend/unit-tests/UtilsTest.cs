namespace WebApp;

public static class UtilsTest
{
    // Read all mock users from file
    private static readonly Arr mockUsers = JSON.Parse(
        File.ReadAllText(FilePath("json", "mock-users.json"))
    );

    [Theory]
    [InlineData("abC9#fgh", true)]  // ok
    [InlineData("stU5/xyz", true)]  // ok too
    [InlineData("abC9#fg", false)]  // too short
    [InlineData("abCd#fgh", false)] // no digit
    [InlineData("abc9#fgh", false)] // no capital letter
    [InlineData("abC9efgh", false)] // no special character
    public static void TestIsPasswordGoodEnough(string password, bool expected)
    {
        Assert.Equal(expected, Utils.IsPasswordGoodEnough(password));
    }

    [Theory]
    [InlineData("abC9#fgh", true)]  // ok
    [InlineData("stU5/xyz", true)]  // ok too
    [InlineData("abC9#fg", false)]  // too short
    [InlineData("abCd#fgh", false)] // no digit
    [InlineData("abc9#fgh", false)] // no capital letter
    [InlineData("abC9efgh", false)] // no special character
    public static void TestIsPasswordGoodEnoughRegexVersion(string password, bool expected)
    {
        Assert.Equal(expected, Utils.IsPasswordGoodEnoughRegexVersion(password));
    }

    [Theory]
    [InlineData(
        "---",
        "Hello, I am going through hell. Hell is a real fucking place " +
            "outside your goddamn comfy tortoiseshell!",
        "Hello, I am going through ---. --- is a real --- place " +
            "outside your --- comfy tortoiseshell!"
    )]
    [InlineData(
        "---",
        "Rhinos have a horny knob? (or what should I call it) on " +
            "their heads. And doorknobs are damn round.",
        "Rhinos have a --- ---? (or what should I call it) on " +
            "their heads. And doorknobs are --- round."
    )]
    public static void TestRemoveBadWords(string replaceWith, string original, string expected)
    {
        Assert.Equal(expected, Utils.RemoveBadWords(original, replaceWith));
    }

    [Fact]
    public static void TestCreateMockUsers()
    {
        // Get all users from the database
        Arr usersInDb = SQLQuery("SELECT email FROM users");
        Arr emailsInDb = usersInDb.Map(user => user.email);
        // Only keep the mock users not already in db
        Arr mockUsersNotInDb = mockUsers.Filter(
            mockUser => !emailsInDb.Contains(mockUser.email)
        );
        // Get the result of running the method in our code
        var result = Utils.CreateMockUsers();
        // Assert that the CreateMockUsers only return
        // newly created users in the db
        Console.WriteLine($"The test expected that {mockUsersNotInDb.Length} users should be added.");
        Console.WriteLine($"And {result.Length} users were added.");
        Console.WriteLine("The test also asserts that the users added are equivalent (the same) as the expected users!");
        Assert.Equivalent(mockUsersNotInDb, result);
        Console.WriteLine("The test passed!");
    }

    [Fact]
    public static void TestRemoveMockUsers()
    {
        var expectedRemovedUsers = Utils.RemoveMockUsers();

        //Kontrollera att expectedRemovedUsers inte innehåller emails från mockUsers
        Assert.Equivalent(expectedRemovedUsers, mockUsers);
    }

    [Fact]
    public static void TestCountDomainsFromUserEmails()
    {
        var expectedResult = Utils.CountDomainsFromUserEmails();

        Arr query = SQLQuery(
            @"SELECT SUBSTR(email, INSTR(email, '@')+1) AS domains,
            COUNT(*) AS counts
            FROM users
            GROUP BY domains
            ;"
        );

        //Lagra SQL-resultatet i objekt
        Obj result = Obj();
        foreach (var domain in query)
        {
            string domains = domain["domains"]; //key => hämtar domänen
            long counts = domain["counts"];    //value => hämtar räknaren

            if (domains != null)
            {
            //Lägg till domänen & dess räknare i resultatobjektet
            result[domains] = counts;
            }
        }
        //Assert.Equal>> result ≠ expectedResult -> sorterad vs osorterad ej jämförtbart 
        Assert.Equivalent(result, expectedResult); //jämförelse i samma format>> obj ≠ arr
    }
}