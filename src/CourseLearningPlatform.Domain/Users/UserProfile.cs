namespace CourseLearningPlatform.Domain.Users;

public class UserProfile
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Email { get; set; }
    public string? PasswordHash { get; set; }
    public required string EmployeeCode { get; set; }
    public DateOnly JoiningDate { get; set; }
    public bool IsAdmin { get; set; }
    
}
