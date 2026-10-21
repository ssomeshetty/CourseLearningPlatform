import { HttpErrorResponse } from '@angular/common/http';

export function errorMessage(error: unknown): string {
  if (error instanceof HttpErrorResponse) {
    if (error.status === 0) return "We couldn't connect to the service. Check your connection and try again.";

    const problem = error.error as { title?: string; detail?: string; traceId?: string; errors?: Record<string, string[] | string> } | string | null;
    if (error.status === 400 && problem && typeof problem === 'object' && problem.errors) {
      if (Object.keys(problem.errors).some(field => field === '$' || field.toLowerCase() === 'request')) {
        return "We couldn't read the request. Check that all fields are filled in correctly and try again.";
      }
      const messages = Object.values(problem.errors)
        .flatMap(value => Array.isArray(value) ? value : [value])
        .filter(Boolean);
      if (messages.length) return messages.slice(0, 3).join(' ');
    }
    if (problem && typeof problem === 'object' && (problem.detail || problem.title)) {
      const message = problem.detail || problem.title || 'Please try again.';
      return error.status >= 500 && problem.traceId
        ? `${message} Reference code: ${problem.traceId}.`
        : message;
    }
    if (typeof problem === 'string' && problem.trim()) return problem;

    switch (error.status) {
      case 400: return 'Please check the information you entered and try again.';
      case 401: return "Your sign-in details are incorrect or your session has expired. Please sign in again.";
      case 403: return "You don't have permission to do that. Contact an administrator if you need access.";
      case 404: return "We couldn't find that item. It may have been removed. Refresh and try again.";
      case 409: return 'This action conflicts with existing information. Refresh the page and try again.';
      case 429: return 'Too many requests. Please wait a moment and try again.';
      case 503: return 'The service is temporarily unavailable. Please try again in a few minutes.';
      default:
        return error.status >= 500
          ? "We couldn't complete your request. Please try again. If the problem continues, contact support."
          : 'Your request could not be completed. Please check the information and try again.';
    }
  }

  return 'Something went wrong. Please try again. If the problem continues, contact support.';
}
