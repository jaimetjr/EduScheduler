import { describe, it, expect } from 'vitest';
import { render, screen } from '@testing-library/react';
import EventList from '../EventList';
import type { StudentEventDto } from '../../types';

const mockEvents: StudentEventDto[] = [
  {
    id: 1,
    subject: 'Math Class',
    bodyPreview: 'Introduction to calculus',
    start: '2026-02-21T09:00:00',
    end: '2026-02-21T10:30:00',
    location: 'Room 101',
    isAllDay: false,
    organizerName: 'Prof. Euler',
  },
  {
    id: 2,
    subject: 'Career Fair',
    bodyPreview: null,
    start: '2026-02-22T09:00:00',
    end: '2026-02-22T17:00:00',
    location: 'Main Hall',
    isAllDay: true,
    organizerName: null,
  },
];

describe('EventList', () => {
  it('shows loading skeletons when loading', () => {
    const { container } = render(
      <EventList events={[]} isLoading={true} />
    );
    const skeletons = container.querySelectorAll('.animate-pulse');
    expect(skeletons.length).toBeGreaterThan(0);
  });

  it('shows empty message when no events', () => {
    render(<EventList events={[]} isLoading={false} />);
    expect(screen.getByText('No events scheduled for this student.')).toBeInTheDocument();
  });

  it('renders event subjects', () => {
    render(<EventList events={mockEvents} isLoading={false} />);
    expect(screen.getByText('Math Class')).toBeInTheDocument();
    expect(screen.getByText('Career Fair')).toBeInTheDocument();
  });

  it('renders body preview when present', () => {
    render(<EventList events={mockEvents} isLoading={false} />);
    expect(screen.getByText('Introduction to calculus')).toBeInTheDocument();
  });

  it('renders location when present', () => {
    render(<EventList events={mockEvents} isLoading={false} />);
    expect(screen.getByText(/Room 101/)).toBeInTheDocument();
  });

  it('renders organizer when present', () => {
    render(<EventList events={mockEvents} isLoading={false} />);
    expect(screen.getByText(/Prof. Euler/)).toBeInTheDocument();
  });

  it('shows all day badge for all day events', () => {
    render(<EventList events={mockEvents} isLoading={false} />);
    expect(screen.getByText('All day')).toBeInTheDocument();
  });
});
