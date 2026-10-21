export interface Session { id: number; name: string; email: string; role: string; accessToken: string; }
export interface Course { id: number; title: string; description: string; instructor: string; imageUrl: string; cost: number; hours: number; }
export interface Page<T> { items: T[]; page: number; pageSize: number; totalCount: number; totalPages: number; }
export interface Enrollment { userId: number; courseId: number; courseTitle: string; status: string; completedAtUtc: string | null; }
export interface UserProfile { id: number; name: string; email: string; employeeCode: string; joiningDate: string; isAdmin: boolean; }
export interface CourseInput { title: string; description: string; instructor: string; imageUrl: string; cost: number | null; hours: number | null; }
export interface CourseAnalytics { courseId: number; courseTitle: string; totalEnrolledLearners: number; startedCount: number; inProgressCount: number; completedCount: number; }
export interface UserAnalytics { userId: number; totalEnrolledCourses: number; notStartedCourses: number; inProgressCourses: number; completedCourses: number; completedCourseHours: number; inProgressEstimatedHours: number; totalEstimatedLearningHours: number; }
