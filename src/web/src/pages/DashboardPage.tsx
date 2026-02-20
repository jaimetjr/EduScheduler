import { useState, useCallback } from 'react';
import Layout from '../components/Layout';
import StudentSearch from '../components/StudentSearch';
import StudentList from '../components/StudentList';
import EventList from '../components/EventList';
import Pagination from '../components/Pagination';
import { useStudents } from '../hooks/useStudents';
import { useEvents } from '../hooks/useEvents';
import type { StudentDto } from '../types';

export default function DashboardPage() {
  const [search, setSearch] = useState('');
  const [studentPage, setStudentPage] = useState(1);
  const [eventPage, setEventPage] = useState(1);
  const [selectedStudent, setSelectedStudent] = useState<StudentDto | null>(null);

  const { data: studentsData, isLoading: studentsLoading } = useStudents(search, studentPage);
  const { data: eventsData, isLoading: eventsLoading } = useEvents(
    selectedStudent?.id ?? null,
    eventPage
  );

  const handleSearch = useCallback((term: string) => {
    setSearch(term);
    setStudentPage(1);
  }, []);

  const handleSelectStudent = useCallback((student: StudentDto) => {
    setSelectedStudent(student);
    setEventPage(1);
  }, []);

  return (
    <Layout>
      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        {/* Left panel: Students */}
        <div className="lg:col-span-1 space-y-4">
          <h2 className="text-lg font-semibold text-gray-800">Students</h2>
          <StudentSearch onSearch={handleSearch} />
          <StudentList
            students={studentsData?.items ?? []}
            selectedId={selectedStudent?.id ?? null}
            onSelect={handleSelectStudent}
            isLoading={studentsLoading}
          />
          {studentsData && (
            <Pagination
              page={studentPage}
              totalPages={studentsData.totalPages}
              onPageChange={setStudentPage}
            />
          )}
          {studentsData && (
            <p className="text-xs text-gray-400 text-center">
              {studentsData.totalCount} student{studentsData.totalCount !== 1 ? 's' : ''} found
            </p>
          )}
        </div>

        {/* Right panel: Events */}
        <div className="lg:col-span-2 space-y-4">
          {selectedStudent ? (
            <>
              <div>
                <h2 className="text-lg font-semibold text-gray-800">
                  Events for {selectedStudent.displayName}
                </h2>
                <p className="text-sm text-gray-500">{selectedStudent.email}</p>
              </div>
              <EventList
                events={eventsData?.items ?? []}
                isLoading={eventsLoading}
              />
              {eventsData && (
                <Pagination
                  page={eventPage}
                  totalPages={eventsData.totalPages}
                  onPageChange={setEventPage}
                />
              )}
              {eventsData && (
                <p className="text-xs text-gray-400 text-center">
                  {eventsData.totalCount} event{eventsData.totalCount !== 1 ? 's' : ''}
                </p>
              )}
            </>
          ) : (
            <div className="flex items-center justify-center h-64 text-gray-400 border-2 border-dashed border-gray-200 rounded-xl">
              <p>Select a student to view their events</p>
            </div>
          )}
        </div>
      </div>
    </Layout>
  );
}
