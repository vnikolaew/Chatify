import React from 'react';
import { render } from '@testing-library/react';

import Page from '../app/page';

describe('Index', () => {
  it('should render successfully', () => {
    const { baseElement } = render(<Page />);
    expect(baseElement).toBeTruthy();
  });
});
