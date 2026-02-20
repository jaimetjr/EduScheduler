import { describe, it, expect } from 'vitest';
import { render, screen } from '@testing-library/react';
import { BrowserRouter } from 'react-router-dom';
import LoginPage from '../LoginPage';

function renderWithRouter() {
    return render(
        <BrowserRouter>
            <LoginPage />
        </BrowserRouter>
    );
}

describe('LoginPage', () => {
    it('renders the login form heading', () => {
        renderWithRouter();
        expect(screen.getByRole('heading', { name: 'Sign in' })).toBeInTheDocument();
    });

    it('renders username and password fields', () => {
        renderWithRouter();
        expect(screen.getByText(/username/i)).toBeInTheDocument();
        expect(screen.getByText(/password/i)).toBeInTheDocument();
        expect(screen.getByRole('textbox')).toBeInTheDocument();
    });

    it('renders the sign in button', () => {
        renderWithRouter();
        expect(screen.getByRole('button', { name: /sign in/i })).toBeInTheDocument();
    });

    it('renders link to register page', () => {
        renderWithRouter();
        expect(screen.getByText(/register/i)).toBeInTheDocument();
    });

    it('has the app title', () => {
        renderWithRouter();
        expect(screen.getByText('EduScheduler')).toBeInTheDocument();
    });
});
