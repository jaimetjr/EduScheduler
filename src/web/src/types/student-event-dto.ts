export interface StudentEventDto {
  id: number;
  subject: string;
  bodyPreview: string | null;
  start: string | null;
  end: string | null;
  location: string | null;
  isAllDay: boolean;
  organizerName: string | null;
}