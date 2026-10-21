using System;
using System.Collections.Generic;


namespace CourseLearningPlatform.Domain.Courses;
public class Course
{
    public int Id { get; set; }
    public required string Title { get; set; }
    public required string Description { get; set; }
    public required string Instructor { get; set; }
    public required string ImageUrl { get; set; }
    public required DateTime CreatedAt { get; set; }
    public required decimal Cost { get; set; }
    public required decimal Hours { get; set; }

}
