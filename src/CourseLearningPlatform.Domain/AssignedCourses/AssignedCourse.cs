using CourseLearningPlatform.Domain.Courses;
using CourseLearningPlatform.Domain.Users;

namespace CourseLearningPlatform.Domain.AssignedCourses;

public enum AssignedCourseStatus
{
    Started,
    InProgress,
    Completed
}

public class AssignedCourse
{
    public int UserId { get; set; }
    public int CourseId { get; set; }
    public AssignedCourseStatus Status { get; set; } = AssignedCourseStatus.Started;
    public DateTime StartedTimestamp { get; set; } = DateTime.UtcNow;
    public DateTime? EndedTimestamp { get; set; }

    public decimal HoursCompleted { get; set; } = 0;

    public UserProfile User { get; set; } = null!;
    public Course Course { get; set; } = null!;
}
