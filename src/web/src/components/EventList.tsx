import type { StudentEventDto } from '../types';

interface EventListProps {
  events: StudentEventDto[];
  isLoading: boolean;
}

function formatDate(dateStr: string | null): string {
  if (!dateStr) return '—';
  const date = new Date(dateStr);
  return date.toLocaleDateString('en-US', {
    weekday: 'short',
    month: 'short',
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit',
  });
}

export default function EventList({ events, isLoading }: EventListProps) {
  if (isLoading) {
    return (
      <div className="space-y-3">
        {[...Array(3)].map((_, i) => (
          <div key={i} className="h-24 bg-gray-200 rounded-lg animate-pulse" />
        ))}
      </div>
    );
  }

  if (events.length === 0) {
    return (
      <div className="text-center py-10 text-gray-500">
        No events scheduled for this student.
      </div>
    );
  }

  return (
    <div className="space-y-3">
      {events.map((event) => (
        <div
          key={event.id}
          className="p-4 bg-white border border-gray-200 rounded-lg"
        >
          <div className="flex items-start justify-between gap-4">
            <div className="flex-1 min-w-0">
              <h4 className="font-medium text-gray-900">{event.subject}</h4>
              {event.bodyPreview && (
                <p className="text-sm text-gray-500 mt-1 line-clamp-2">
                  {event.bodyPreview}
                </p>
              )}
              <div className="flex flex-wrap gap-x-4 gap-y-1 mt-2 text-xs text-gray-500">
                <span>📅 {formatDate(event.start)}</span>
                {event.end && <span>→ {formatDate(event.end)}</span>}
                {event.location && <span>📍 {event.location}</span>}
                {event.organizerName && <span>👤 {event.organizerName}</span>}
              </div>
            </div>
            {event.isAllDay && (
              <span className="shrink-0 text-xs bg-indigo-100 text-indigo-700 px-2 py-0.5 rounded-full">
                All day
              </span>
            )}
          </div>
        </div>
      ))}
    </div>
  );
}
