import { useState, useEffect, useRef } from 'react';

interface StudentSearchProps {
  onSearch: (term: string) => void;
}

export default function StudentSearch({ onSearch }: StudentSearchProps) {
  const [value, setValue] = useState('');
  const onSearchRef = useRef(onSearch);
  onSearchRef.current = onSearch;

  useEffect(() => {
    const timeout = setTimeout(() => {
      onSearchRef.current(value);
    }, 300);

    return () => clearTimeout(timeout);
  }, [value]);

  return (
    <input
      type="text"
      placeholder="Search students by name or email..."
      value={value}
      onChange={(e) => setValue(e.target.value)}
      className="w-full px-4 py-2.5 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:border-transparent text-sm"
    />
  );
}
