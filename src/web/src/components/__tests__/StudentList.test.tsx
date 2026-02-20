import { describe, it, expect, vi } from 'vitest';
import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import StudentList from '../StudentList';
import type { StudentDto } from '../../types';

const mockStudents: StudentDto[] = [
  { id: 1, displayName: 'Alice Johnson', email: 'alice@school.edu', department: 'Engineering' },
  { id: 2, displayName: 'Bob Smith', email: 'bob@school.edu', department: null },
];

describe('StudentList', () => {
  it('shows loading skeletons when loading', () => {
    const { container } = render(
      <StudentList students={[]} selectedId={null} onSelect={() => {}} isLoading={true} />
    );
    const skeletons = container.querySelectorAll('.animate-pulse');
    expect(skeletons.length).toBeGreaterThan(0);
  });

  it('shows empty message when no students', () => {
    render(
      <StudentList students={[]} selectedId={null} onSelect={() => {}} isLoading={false} />
    );
    expect(screen.getByText('No students found.')).toBeInTheDocument();
  });

  it('renders student names and emails', () => {
    render(
      <StudentList students={mockStudents} selectedId={null} onSelect={() => {}} isLoading={false} />
    );
    expect(screen.getByText('Alice Johnson')).toBeInTheDocument();
    expect(screen.getByText('alice@school.edu')).toBeInTheDocument();
    expect(screen.getByText('Bob Smith')).toBeInTheDocument();
  });

  it('renders department badge when present', () => {
    render(
      <StudentList students={mockStudents} selectedId={null} onSelect={() => {}} isLoading={false} />
    );
    expect(screen.getByText('Engineering')).toBeInTheDocument();
  });

  it('highlights selected student', () => {
    render(
      <StudentList students={mockStudents} selectedId={1} onSelect={() => {}} isLoading={false} />
    );
    const buttons = screen.getAllByRole('button');
    expect(buttons[0].className).toContain('border-indigo-500');
    expect(buttons[1].className).not.toContain('border-indigo-500');
  });

  it('calls onSelect when student is clicked', async () => {
    const user = userEvent.setup();
    const onSelect = vi.fn();
    render(
      <StudentList students={mockStudents} selectedId={null} onSelect={onSelect} isLoading={false} />
    );

    await user.click(screen.getByText('Alice Johnson'));
    expect(onSelect).toHaveBeenCalledWith(mockStudents[0]);
  });
});
