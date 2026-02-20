import { describe, it, expect, vi } from 'vitest';
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import StudentSearch from '../StudentSearch';

describe('StudentSearch', () => {
  it('renders the search input', () => {
    render(<StudentSearch onSearch={() => {}} />);
    expect(screen.getByPlaceholderText(/search students/i)).toBeInTheDocument();
  });

  it('calls onSearch after debounce', async () => {
    const onSearch = vi.fn();
    render(<StudentSearch onSearch={onSearch} />);
    const input = screen.getByPlaceholderText(/search students/i);

    fireEvent.change(input, { target: { value: 'alice' } });

    await waitFor(() => {
      expect(onSearch).toHaveBeenCalledWith('alice');
    }, { timeout: 1000 });
  });

  it('updates input value on change', () => {
    render(<StudentSearch onSearch={() => {}} />);
    const input = screen.getByPlaceholderText(/search students/i) as HTMLInputElement;

    fireEvent.change(input, { target: { value: 'bob' } });
    expect(input.value).toBe('bob');
  });
});
