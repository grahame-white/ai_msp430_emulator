---
name: Defect Triage
about: Internal template for defect triage and assignment
title: '[TRIAGE] '
labels: ['triage', 'internal']
assignees: ''
---

## Defect Triage Information
**Original Issue Link:** #[issue_number]

## Triage Assessment
**Priority Level:**
- [ ] P0 - Critical (System down, data loss, security issue)
- [ ] P1 - High (Major feature broken, significant user impact)
- [ ] P2 - Medium (Minor feature issues, workaround available)
- [ ] P3 - Low (Cosmetic, documentation, enhancement)

**Severity Classification:**
- [ ] Critical - Complete system failure, data corruption, security vulnerability
- [ ] High - Major functionality broken, significant performance impact
- [ ] Medium - Minor functionality issues, cosmetic problems with functional impact
- [ ] Low - Documentation, cosmetic issues without functional impact

**Component Affected:**
- [ ] CPU Core
- [ ] Memory System
- [ ] Instruction Set
- [ ] Peripherals
- [ ] Configuration
- [ ] Logging
- [ ] Testing Infrastructure
- [ ] Documentation
- [ ] Build System
- [ ] Other: ____________

## Assignment Details
**Assigned To:** @[username]
**Estimated Effort:**
- [ ] Small (< 4 hours)
- [ ] Medium (4-16 hours)
- [ ] Large (16+ hours)

**Target Sprint/Milestone:** [sprint_name]
**Due Date:** [yyyy-mm-dd]

## Root Cause Analysis
**Preliminary Investigation:**
- [ ] Code review completed
- [ ] Test case created
- [ ] Environment reproduced
- [ ] Impact assessment completed

**Suspected Root Cause:**
- [ ] Logic error
- [ ] Configuration issue
- [ ] Environmental factor
- [ ] Integration problem
- [ ] Performance issue
- [ ] Documentation gap
- [ ] Unknown/Requires investigation

## Resolution Plan
**Approach:**
1. [ ] Investigate root cause
2. [ ] Implement fix
3. [ ] Create/update tests
4. [ ] Update documentation
5. [ ] Verify fix

**Testing Strategy:**
- [ ] Unit tests added/updated
- [ ] Integration tests added/updated
- [ ] Manual testing required
- [ ] Performance testing required
- [ ] Regression tests created

## Dependencies
**Blocked By:** [list any blocking issues]
**Blocks:** [list any issues this blocks]
**Related Issues:** [list related issues]

## Risk Assessment
**Risk Level:**
- [ ] Low - Isolated change, well understood
- [ ] Medium - Moderate complexity, some unknowns
- [ ] High - Complex change, significant unknowns

**Mitigation Strategy:**
[Describe risk mitigation approach]

## Communication Plan
**Stakeholders to Notify:**
- [ ] Product Owner
- [ ] Development Team
- [ ] QA Team
- [ ] Documentation Team
- [ ] Users (if customer-facing)

**Communication Method:**
- [ ] Issue updates
- [ ] Email notification
- [ ] Slack/Teams message
- [ ] Sprint review
- [ ] Release notes

## Acceptance Criteria
**Definition of Done:**
- [ ] Root cause identified and documented
- [ ] Fix implemented and code reviewed
- [ ] Tests passing (unit, integration, regression)
- [ ] Documentation updated
- [ ] Fix verified in target environment
- [ ] Stakeholders notified
- [ ] Issue closed with resolution summary

## Notes
**Additional Information:**
[Any additional context, links, or notes for the assignee]

**Triage Date:** [yyyy-mm-dd]
**Triaged By:** @[triage_username]