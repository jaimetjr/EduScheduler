import type { StudentDto } from '../types';

interface StudentListProps {
  students: StudentDto[];
  selectedId: number | null;
  onSelect: (student: StudentDto) => void;
  isLoading: boolean;
}

export default function StudentList({ students, selectedId, onSelect, isLoading }: StudentListProps) {
  if (isLoading) {
    return (
      <div className="space-y-3">
        {[...Array(5)].map((_, i) => (
          <div key={i} className="h-16 bg-gray-200 rounded-lg animate-pulse" />
        ))}
      </div>
    );
  }

  if (students.length === 0) {
    return (
      <div className="text-center py-10 text-gray-500">
        No students found.
      </div>
    );
  }

  return (
    <div className="space-y-2">
      {students.map((student) => (
        <button
          key={student.id}
          onClick={() => onSelect(student)}
          className={`w-full text-left p-4 rounded-lg border transition-colors cursor-pointer ${
            selectedId === student.id
              ? 'border-indigo-500 bg-indigo-50'
              : 'border-gray-200 bg-white hover:border-indigo-300 hover:bg-gray-50'
          }`}
        >
          <p className="font-medium text-gray-900">{student.displayName}</p>
          <p className="text-sm text-gray-500">{student.email}</p>
          {student.department && (
            <span className="inline-block mt-1 text-xs bg-gray-100 text-gray-600 px-2 py-0.5 rounded-full">
              {student.department}
            </span>
          )}
        </button>
      ))}
    </div>
  );
}
