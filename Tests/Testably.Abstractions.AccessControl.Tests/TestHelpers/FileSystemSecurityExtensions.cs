using System.Collections.Generic;
using System.Security.AccessControl;
using System.Security.Principal;

namespace Testably.Abstractions.AccessControl.Tests.TestHelpers;

internal static class FileSystemSecurityExtensions
{
	/// <summary>
	///     Compares to <see cref="FileSystemSecurity" /> objects.
	///     https://stackoverflow.com/a/17047098
	/// </summary>
#pragma warning disable CA1416
	public static bool HasSameAccessRightsAs(this FileSystemSecurity sourceAccessControl,
	                                         FileSystemSecurity destinationAccessControl)
	{
		// combine parent access rules
		Dictionary<IdentityReference, FileSystemRights> combinedParentAccessAllowRules =
			new();
		Dictionary<IdentityReference, FileSystemRights> combinedParentAccessDenyRules =
			new();
		foreach (FileSystemAccessRule parentAccessRule in sourceAccessControl
		   .GetAccessRules(true, true, typeof(NTAccount)))
		{
			if (parentAccessRule.AccessControlType == AccessControlType.Allow)
			{
				if (combinedParentAccessAllowRules.ContainsKey(parentAccessRule
				   .IdentityReference))
				{
					combinedParentAccessAllowRules[parentAccessRule.IdentityReference] |=
						parentAccessRule.FileSystemRights;
				}
				else
				{
					combinedParentAccessAllowRules.Add(parentAccessRule.IdentityReference,
						parentAccessRule.FileSystemRights);
				}
			}
			else if (combinedParentAccessDenyRules.ContainsKey(parentAccessRule
			   .IdentityReference))
			{
				combinedParentAccessDenyRules[parentAccessRule.IdentityReference] |=
					parentAccessRule.FileSystemRights;
			}
			else
			{
				combinedParentAccessDenyRules.Add(parentAccessRule.IdentityReference,
					parentAccessRule.FileSystemRights);
			}
		}

		// combine child access rules

		Dictionary<IdentityReference, FileSystemRights> combinedChildAccessAllowRules =
			new();
		Dictionary<IdentityReference, FileSystemRights> combinedChildAccessDenyRules =
			new();
		foreach (FileSystemAccessRule childAccessRule in destinationAccessControl
		   .GetAccessRules(true, true, typeof(NTAccount)))
		{
			if (childAccessRule.AccessControlType == AccessControlType.Allow)
			{
				if (combinedChildAccessAllowRules.ContainsKey(childAccessRule
				   .IdentityReference))
				{
					combinedChildAccessAllowRules[childAccessRule.IdentityReference] |=
						childAccessRule.FileSystemRights;
				}
				else
				{
					combinedChildAccessAllowRules.Add(childAccessRule.IdentityReference,
						childAccessRule.FileSystemRights);
				}
			}
			else if (combinedChildAccessDenyRules.ContainsKey(childAccessRule
			   .IdentityReference))
			{
				combinedChildAccessDenyRules[childAccessRule.IdentityReference] |=
					childAccessRule.FileSystemRights;
			}
			else
			{
				combinedChildAccessDenyRules.Add(childAccessRule.IdentityReference,
					childAccessRule.FileSystemRights);
			}
		}

		// compare combined rules
		Dictionary<IdentityReference, FileSystemRights> accessAllowRulesGainedByChild =
			new();
		foreach (
			KeyValuePair<IdentityReference, FileSystemRights> combinedChildAccessAllowRule
			in combinedChildAccessAllowRules)
		{
			if (combinedParentAccessAllowRules.ContainsKey(combinedChildAccessAllowRule
			   .Key))
			{
				FileSystemRights accessAllowRuleGainedByChild =
					combinedChildAccessAllowRule.Value &
					~combinedParentAccessAllowRules[combinedChildAccessAllowRule.Key];
				if (accessAllowRuleGainedByChild != default)
				{
					accessAllowRulesGainedByChild.Add(combinedChildAccessAllowRule.Key,
						accessAllowRuleGainedByChild);
				}
			}
			else
			{
				accessAllowRulesGainedByChild.Add(combinedChildAccessAllowRule.Key,
					combinedChildAccessAllowRule.Value);
			}
		}

		Dictionary<IdentityReference, FileSystemRights> accessDenyRulesGainedByChild =
			new();
		foreach (
			KeyValuePair<IdentityReference, FileSystemRights> combinedChildAccessDenyRule
			in combinedChildAccessDenyRules)
		{
			if (combinedParentAccessDenyRules.ContainsKey(combinedChildAccessDenyRule
			   .Key))
			{
				FileSystemRights accessDenyRuleGainedByChild =
					combinedChildAccessDenyRule.Value &
					~combinedParentAccessDenyRules[combinedChildAccessDenyRule.Key];
				if (accessDenyRuleGainedByChild != default)
				{
					accessDenyRulesGainedByChild.Add(combinedChildAccessDenyRule.Key,
						accessDenyRuleGainedByChild);
				}
			}
			else
			{
				accessDenyRulesGainedByChild.Add(combinedChildAccessDenyRule.Key,
					combinedChildAccessDenyRule.Value);
			}
		}

		Dictionary<IdentityReference, FileSystemRights> accessAllowRulesGainedByParent =
			new();
		foreach (
			KeyValuePair<IdentityReference, FileSystemRights>
				combinedParentAccessAllowRule in combinedParentAccessAllowRules)
		{
			if (combinedChildAccessAllowRules.ContainsKey(combinedParentAccessAllowRule
			   .Key))
			{
				FileSystemRights accessAllowRuleGainedByParent =
					combinedParentAccessAllowRule.Value &
					~combinedChildAccessAllowRules[combinedParentAccessAllowRule.Key];
				if (accessAllowRuleGainedByParent != default)
				{
					accessAllowRulesGainedByParent.Add(combinedParentAccessAllowRule.Key,
						accessAllowRuleGainedByParent);
				}
			}
			else
			{
				accessAllowRulesGainedByParent.Add(combinedParentAccessAllowRule.Key,
					combinedParentAccessAllowRule.Value);
			}
		}

		Dictionary<IdentityReference, FileSystemRights> accessDenyRulesGainedByParent =
			new();
		foreach (
			KeyValuePair<IdentityReference, FileSystemRights> combinedParentAccessDenyRule
			in combinedParentAccessDenyRules)
		{
			if (combinedChildAccessDenyRules.ContainsKey(combinedParentAccessDenyRule
			   .Key))
			{
				FileSystemRights accessDenyRuleGainedByParent =
					combinedParentAccessDenyRule.Value &
					~combinedChildAccessDenyRules[combinedParentAccessDenyRule.Key];
				if (accessDenyRuleGainedByParent != default)
				{
					accessDenyRulesGainedByParent.Add(combinedParentAccessDenyRule.Key,
						accessDenyRuleGainedByParent);
				}
			}
			else
			{
				accessDenyRulesGainedByParent.Add(combinedParentAccessDenyRule.Key,
					combinedParentAccessDenyRule.Value);
			}
		}

		if (accessAllowRulesGainedByChild.Count > 0 ||
		    accessDenyRulesGainedByChild.Count > 0 ||
		    accessAllowRulesGainedByParent.Count > 0 ||
		    accessDenyRulesGainedByParent.Count > 0)
		{
			return false;
		}

		return true;
	}
#pragma warning restore CA1416
}