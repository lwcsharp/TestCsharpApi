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
        Arr initialDb = SQLQuery("SELECT * FROM users");
        var test = initialDb[0];

        Arr expectedRemovedUsers = Utils.RemoveMockUsers();
        Arr expectedEmails = expectedRemovedUsers.Map(user => user.email);
        Arr expectedPasswords = expectedRemovedUsers.Map(user => user.password);
        
        Arr usersInDb = SQLQuery("SELECT email FROM users");
        Arr emailsInDb = usersInDb.Map(user => user.email);

        //Kontrollera att ingen av de förväntade e-postadresserna finns kvar i databasen
        foreach (var email in expectedEmails)
        {
            Assert.DoesNotContain(email, emailsInDb);
        }

        //Assert.True(expectedPasswords.Length > 0);

        //Kontrollera att inget lösenord finns kvar för de borttagna användarna
        foreach (var user in expectedRemovedUsers)
        {
            if (!emailsInDb.Contains(user.email))
            {
                Assert.True(user.HasKey("password"), $"Password should be included for user: {user.id}");
            }
            else
            {
                Assert.Fail($"User ID:{user.id}, password should still exist in the database.");
            }        
        } 
        
        //Kontrollera att det faktiska antalet användare efter borttagning stämmer
        int initialCount = initialDb.Count();
        int removedCount = expectedRemovedUsers.Count();
        int expectedCount = initialCount - removedCount;
        Assert.Equal(expectedCount, usersInDb.Count());    
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