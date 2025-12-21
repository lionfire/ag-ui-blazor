# Epic 04-002: Performance Virtualization

**Phase**: 04 - Polish & Extensibility
**Estimated Effort**: 3-4 days

## Overview
Implement virtual scrolling for large conversations using MudVirtualize.

## Implementation Tasks
- [ ] Add EnableVirtualization parameter to MudMessageList
- [ ] Implement MudVirtualize for message list
- [ ] Configure item size and overscan
- [ ] Handle dynamic message heights
- [ ] Add MaxMessageSize parameter (warn if exceeded)
- [ ] Test with 10,000+ messages
- [ ] Measure performance improvement
- [ ] Make opt-in (off by default)

## Acceptance Criteria
- [ ] Virtualization handles 10,000+ messages
- [ ] Performance measured and documented
- [ ] Opt-in configuration works
