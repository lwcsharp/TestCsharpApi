using Microsoft.AspNetCore.Components;

namespace WebApp;

public static class Utils
{
    // Read all mock users from file
    private static readonly Arr mockUsers = JSON.Parse(
        File.ReadAllText(FilePath("json", "mock-users.json"))
    );

    // Read all bad words from file and sort from longest to shortest
    // if we didn't sort we would often get "---ing" instead of "---" etc.
    // (Comment out the sort - run the unit tests and see for yourself...)
    private static readonly Arr badWords = ((Arr)JSON.Parse(
        File.ReadAllText(FilePath("json", "bad-words.json"))
    )).Sort((a, b) => ((string)b).Length - ((string)a).Length);

    public static bool IsPasswordGoodEnough(string password)
    {
        return password.Length >= 8
            && password.Any(Char.IsDigit)
            && password.Any(Char.IsLower)
            && password.Any(Char.IsUpper)
            && password.Any(x => !Char.IsLetterOrDigit(x));
    }

    public static bool IsPasswordGoodEnoughRegexVersion(string password)
    {
        // See: https://dev.to/rasaf_ibrahim/write-regex-password-validation-like-a-pro-5175
        var pattern = @"^(?=.*[0-9])(?=.*[a-zåäö])(?=.*[A-ZÅÄÖ])(?=.*\W).{8,}$";
        return Regex.IsMatch(password, pattern);
    }

    public static string RemoveBadWords(string comment, string replaceWith = "---")
    {
        comment = " " + comment;
        replaceWith = " " + replaceWith + "$1";
        badWords.ForEach(bad =>
        {
            var pattern = @$" {bad}([\,\.\!\?\:\; ])";
            comment = Regex.Replace(
                comment, pattern, replaceWith, RegexOptions.IgnoreCase);
        });
        return comment[1..];
    }

    public static Arr CreateMockUsers()
    {
        Arr successFullyWrittenUsers = Arr();
        foreach (var user in mockUsers)
        {
            user.password = "12345678";
            var result = SQLQueryOne(
                @"INSERT INTO users(firstName,lastName,email,password)
                VALUES($firstName, $lastName, $email, $password)
            ", user);
            // If we get an error from the DB then we haven't added
            // the mock users, if not we have so add to the successful list
            if (!result.HasKey("error"))
            {
                // The specification says return the user list without password
                user.Delete("password");
                successFullyWrittenUsers.Push(user);
            }
        }
        return successFullyWrittenUsers;
    }

    public static Arr RemoveMockUsers()
    {
        Arr removedUsersExclPassword = Arr();

        foreach (var user in mockUsers)
        {
            var query = $"DELETE FROM users WHERE email = '{user.email}'";
            var result = SQLQueryOne(query);

            if (!result.HasKey("error"))
            {
                user.Delete("password");
                removedUsersExclPassword.Push(user);
            }
        }
        return removedUsersExclPassword;
    }

    public static Obj CountDomainsFromUserEmails()
    {
        //Skapa ett nytt objekt för att lagra domänräkningarna
        Obj domainCounts = Obj();

        //Lagra alla användare från DB i lista av användarobjekt
        Arr usersInDB = SQLQuery(
            @"SELECT email
            FROM users"
        );

        foreach (var user in usersInDB)
        {
            string email = user.email;
            string domain = email.Substring(email.IndexOf('@') + 1); //+1, metoden börjar extrahera från tecknet efter @ >> index = 0

            //Kontrollera om denna domän redan finns i domainCounts
            if (domainCounts.HasKey(domain))
            {
                //Om domänen redan finns, öka räkningen för denna domän med 1
                domainCounts[domain]++;
            }
            else
            {
                //Om domänen inte finns, lägg till den med en räkning på 1
                domainCounts[domain] = 1;
            }
        }
        //Returnera objektet som innehåller domäner och deras respektive räkningar
        return domainCounts;
    }
}