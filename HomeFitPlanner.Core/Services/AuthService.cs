using System;
using System.Security.Cryptography;
using System.Text;
using HomeFitPlanner.Core.DataStructures.HashTables;
using HomeFitPlanner.Core.DataStructures.Trees;
using HomeFitPlanner.Core.Models;

namespace HomeFitPlanner.Core.Services;

/// <summary>
/// DSA-backed Authentication and User Credential Service.
/// Relies on:
/// 1. Hand-written ChainingHashTable for O(1) user credential lookup by email/username.
/// 2. Hand-written Prefix Tree (Trie) for O(L) real-time username collision detection.
/// 3. Cryptographic Salted SHA-256 hashing for timing-attack-safe O(1) password verification.
/// </summary>
public class AuthService
{
    private static AuthService? _instance;
    public static AuthService Instance => _instance ??= new AuthService();

    private readonly ChainingHashTable<string, UserAccount> _userTable = new(capacity: 17);
    private readonly Trie _usernameTrie = new();

    public UserAccount? CurrentUser { get; set; }

    public AuthService()
    {
        SeedDefaultAccounts();
    }

    private void SeedDefaultAccounts()
    {
        // 1. Athlete 1 Profile (Beginner / Weight Loss)
        var athlete1 = CreateAccount(
            name: "Athlete 1",
            email: "demo1@homefit.edu",
            username: "athlete1",
            password: "password123",
            goal: GoalType.LoseWeight,
            level: FitnessLevel.Beginner,
            age: 25,
            gender: GenderType.Female,
            weightKg: 65,
            heightCm: 162
        );
        RegisterAccountInternal(athlete1);

        // 2. Athlete 2 Profile (Intermediate / Muscle Build)
        var athlete2 = CreateAccount(
            name: "Athlete 2",
            email: "demo2@homefit.edu",
            username: "athlete2",
            password: "password123",
            goal: GoalType.BuildMuscle,
            level: FitnessLevel.Intermediate,
            age: 24,
            gender: GenderType.Male,
            weightKg: 72,
            heightCm: 175
        );
        RegisterAccountInternal(athlete2);

        // 3. Athlete 3 Profile (Beginner / Stay Fit)
        var athlete3 = CreateAccount(
            name: "Athlete 3",
            email: "demo3@homefit.edu",
            username: "athlete3",
            password: "password123",
            goal: GoalType.StayFit,
            level: FitnessLevel.Beginner,
            age: 28,
            gender: GenderType.Female,
            weightKg: 58,
            heightCm: 160
        );
        RegisterAccountInternal(athlete3);

        // 4. Athlete 4 Profile (Intermediate / Cardio)
        var athlete4 = CreateAccount(
            name: "Athlete 4",
            email: "demo4@homefit.edu",
            username: "athlete4",
            password: "password123",
            goal: GoalType.BoostCardio,
            level: FitnessLevel.Intermediate,
            age: 26,
            gender: GenderType.Male,
            weightKg: 68,
            heightCm: 170
        );
        RegisterAccountInternal(athlete4);

        // Set default current user
        CurrentUser = athlete1;
    }

    private UserAccount CreateAccount(string name, string email, string username, string password, GoalType goal, FitnessLevel level, int age, GenderType gender, double weightKg, double heightCm)
    {
        byte[] saltBytes = RandomNumberGenerator.GetBytes(16);
        string salt = Convert.ToBase64String(saltBytes);
        string hash = HashPassword(password, salt);

        var profile = new UserProfile
        {
            Name = name,
            Age = age,
            Gender = gender,
            HeightCm = heightCm,
            WeightKg = weightKg,
            Goal = goal,
            StartingLevel = level,
            WorkoutDaysPerWeek = 3,
            MinutesPerSession = 30,
            HasChairOrBench = true,
            HasKneeLimitation = true
        };

        return new UserAccount
        {
            Name = name,
            Email = email.ToLowerInvariant().Trim(),
            Username = username.ToLowerInvariant().Trim(),
            PasswordHash = hash,
            PasswordSalt = salt,
            Profile = profile
        };
    }

    private void RegisterAccountInternal(UserAccount account)
    {
        _userTable.Insert(account.Email, account);
        _userTable.Insert(account.Username, account);
        _usernameTrie.Insert(account.Username);
        _usernameTrie.Insert(account.Email);
    }

    public (bool Success, string Message, UserAccount? User) SignUp(
        string name,
        string email,
        string username,
        string password,
        string goalStr,
        string levelStr,
        int age = 22,
        string genderStr = "Female",
        double weightKg = 65,
        double heightCm = 162)
    {
        if (string.IsNullOrWhiteSpace(name))
            return (false, "Full name is required.", null);
        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
            return (false, "Valid email address is required.", null);
        if (string.IsNullOrWhiteSpace(username) || username.Length < 3)
            return (false, "Username must be at least 3 characters.", null);
        if (string.IsNullOrWhiteSpace(password) || password.Length < 6)
            return (false, "Password must be at least 6 characters.", null);

        string normUser = username.Trim().ToLowerInvariant();
        string normEmail = email.Trim().ToLowerInvariant();

        // O(L) Trie availability check
        if (_usernameTrie.Contains(normUser, out int trieDepth))
        {
            return (false, $"Username '{normUser}' is already registered in the Trie index (Depth: {trieDepth}).", null);
        }

        if (_userTable.ContainsKey(normEmail))
        {
            return (false, $"Email '{normEmail}' is already indexed in the Hash Table.", null);
        }

        Enum.TryParse<GoalType>(goalStr, true, out var goal);
        Enum.TryParse<FitnessLevel>(levelStr, true, out var level);
        Enum.TryParse<GenderType>(genderStr, true, out var gender);

        var account = CreateAccount(name.Trim(), normEmail, normUser, password, goal, level, age, gender, weightKg, heightCm);
        RegisterAccountInternal(account);

        CurrentUser = account;
        DataManager.Instance.CurrentProfile = account.Profile;

        // Auto-generate fresh plan for the newly registered user
        var generator = new PlanGeneratorService(DataManager.Instance);
        var plan = generator.GeneratePlan(account.Profile);
        DataManager.Instance.CurrentPlan = plan;
        DataManager.Instance.SavePlan(plan);

        return (true, "Account registered successfully via Hash Table insertion and Trie indexing!", account);
    }

    public (bool Success, string Message, UserAccount? User) LogIn(string identifier, string password)
    {
        if (string.IsNullOrWhiteSpace(identifier) || string.IsNullOrWhiteSpace(password))
            return (false, "Email/username and password are required.", null);

        string normId = identifier.Trim().ToLowerInvariant();

        // O(1) Average-time Hash Table retrieval
        if (!_userTable.TryGetValue(normId, out var account) || account == null)
        {
            return (false, $"Account '{identifier}' not found in the Hash Table index.", null);
        }

        string expectedHash = HashPassword(password, account.PasswordSalt);
        if (!CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(account.PasswordHash),
            Encoding.UTF8.GetBytes(expectedHash)))
        {
            return (false, "Invalid credentials. Password hash verification failed.", null);
        }

        CurrentUser = account;
        DataManager.Instance.CurrentProfile = account.Profile;

        // Ensure user's plan exists
        if (DataManager.Instance.CurrentPlan == null)
        {
            var generator = new PlanGeneratorService(DataManager.Instance);
            var plan = generator.GeneratePlan(account.Profile);
            DataManager.Instance.CurrentPlan = plan;
            DataManager.Instance.SavePlan(plan);
        }

        return (true, $"Welcome back, {account.Name}! Authenticated via O(1) Hash Table lookup.", account);
    }

    public void LogOut()
    {
        CurrentUser = null;
    }

    public object CheckUsernameTrie(string username)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            return new { available = false, depth = 0, message = "Enter username" };
        }

        string norm = username.Trim().ToLowerInvariant();
        bool exists = _usernameTrie.Contains(norm, out int depth);
        return new
        {
            username = norm,
            available = !exists,
            trieDepthTraversed = depth,
            timeComplexity = "O(L) - Prefix Tree Character Path",
            message = exists ? "Taken in Trie" : "Available in Trie"
        };
    }

    public object GetDsaAuthStats(string? query = null)
    {
        int bucketIndex = -1;
        int computedHash = 0;
        if (!string.IsNullOrWhiteSpace(query))
        {
            string s = query.Trim().ToLowerInvariant();
            unchecked
            {
                int hash = 5381;
                for (int i = 0; i < s.Length; i++)
                {
                    hash = ((hash << 5) + hash) ^ s[i];
                }
                computedHash = hash;
                bucketIndex = hash % _userTable.BucketCount;
                if (bucketIndex < 0) bucketIndex += _userTable.BucketCount;
            }
        }

        return new
        {
            userCount = _userTable.Count,
            bucketCount = _userTable.BucketCount,
            loadFactor = Math.Round(_userTable.LoadFactor, 3),
            totalCollisions = _userTable.TotalCollisions,
            trieIndexedWords = _usernameTrie.WordCount,
            hashAlgorithm = "DJB2 Polynomial Rolling Hash (h = ((h << 5) + h) ^ c)",
            collisionResolution = "Separate Chaining (Singly Linked List Buckets)",
            query = query,
            computedHash = computedHash,
            targetBucketIndex = bucketIndex,
            complexity = new
            {
                loginLookup = "O(1) Average Time via Chaining Hash Table",
                signUpInsertion = "O(1) Hash Table Insert + O(L) Trie Insert",
                usernameCheck = "O(L) Prefix Tree Traversal (L = character length)",
                passwordVerify = "O(1) Constant-Time Salted SHA-256 Comparison"
            }
        };
    }

    private static string HashPassword(string password, string salt)
    {
        byte[] saltBytes = Convert.FromBase64String(salt);
        byte[] passBytes = Encoding.UTF8.GetBytes(password);

        byte[] combined = new byte[saltBytes.Length + passBytes.Length];
        Buffer.BlockCopy(saltBytes, 0, combined, 0, saltBytes.Length);
        Buffer.BlockCopy(passBytes, 0, combined, saltBytes.Length, passBytes.Length);

        byte[] hash = SHA256.HashData(combined);
        return Convert.ToBase64String(hash);
    }
}
