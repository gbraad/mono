//
// System.Web.Security.MembershipProvider
//
// Authors:
//	Ben Maurer (bmaurer@users.sourceforge.net)
//	Lluis Sanchez Gual (lluis@novell.com)
//
// (C) 2003 Ben Maurer
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

#if NET_2_0
using System.ComponentModel;
using System.Configuration.Provider;
using System.Web.Configuration;
using System.Security.Cryptography;
using System.Text;

namespace System.Web.Security
{
	public abstract class MembershipProvider : ProviderBase
	{
		static readonly object validatingPasswordEvent = new object ();

		EventHandlerList events = new EventHandlerList ();
		public event MembershipValidatePasswordEventHandler ValidatingPassword {
			add { events.AddHandler (validatingPasswordEvent, value); }
			remove { events.RemoveHandler (validatingPasswordEvent, value); }
		}
		
		protected MembershipProvider ()
		{
		}
		
		public abstract bool ChangePassword (string name, string oldPwd, string newPwd);
		public abstract bool ChangePasswordQuestionAndAnswer (string name, string password, string newPwdQuestion, string newPwdAnswer);
		public abstract MembershipUser CreateUser (string username, string password, string email, string pwdQuestion, string pwdAnswer, bool isApproved, object providerUserKey, out MembershipCreateStatus status);
		public abstract bool DeleteUser (string name, bool deleteAllRelatedData);
		public abstract MembershipUserCollection FindUsersByEmail (string emailToMatch, int pageIndex, int pageSize, out int totalRecords);
		public abstract MembershipUserCollection FindUsersByName (string nameToMatch, int pageIndex, int pageSize, out int totalRecords);
		public abstract MembershipUserCollection GetAllUsers (int pageIndex, int pageSize, out int totalRecords);
		public abstract int GetNumberOfUsersOnline ();
		public abstract string GetPassword (string name, string answer);
		public abstract MembershipUser GetUser (string name, bool userIsOnline);
		public abstract MembershipUser GetUser (object providerUserKey, bool userIsOnline);
		public abstract string GetUserNameByEmail (string email);
		public abstract string ResetPassword (string name, string answer);
		public abstract void UpdateUser (MembershipUser user);
		public abstract bool ValidateUser (string name, string password);
		public abstract bool UnlockUser (string userName);
		
		public abstract string ApplicationName { get; set; }
		public abstract bool EnablePasswordReset { get; }
		public abstract bool EnablePasswordRetrieval { get; }
		public abstract bool RequiresQuestionAndAnswer { get; }
		public abstract int MaxInvalidPasswordAttempts { get; }
		public abstract int MinRequiredNonAlphanumericCharacters { get; }
		public abstract int MinRequiredPasswordLength { get; }
		public abstract int PasswordAttemptWindow { get; }
		public abstract MembershipPasswordFormat PasswordFormat { get; }
		public abstract string PasswordStrengthRegularExpression { get; }
		public abstract bool RequiresUniqueEmail { get; }
		
		protected virtual void OnValidatingPassword (ValidatePasswordEventArgs args)
		{
			MembershipValidatePasswordEventHandler eh = events [validatingPasswordEvent] as MembershipValidatePasswordEventHandler;
			if (eh != null)
				eh (this, args);
		}

		SymmetricAlgorithm GetAlgorithm ()
		{
			MachineKeySection section = MachineKeySection.Config;

			if (section.DecryptionKey.StartsWith ("AutoGenerate"))
				throw new ProviderException ("You must explicitly specify a decryption key in the <machineKey> section when using encrypted passwords.");

			SymmetricAlgorithm sa = section.GetDecryptionAlgorithm ();
			if (sa == null)
				throw new ProviderException (String.Format ("Unsupported decryption attribute '{0}' in <machineKey> configuration section", section.Decryption));

			sa.Key = section.GetDecryptionKey ();
			return sa;
		}

		internal const int SALT_BYTES = 16;
		protected virtual byte [] DecryptPassword (byte [] encodedPassword)
		{
			using (SymmetricAlgorithm sa = GetAlgorithm ()) {
				return MachineKeySectionUtils.Decrypt (sa, encodedPassword, 0, encodedPassword.Length);
			}
		}

		protected virtual byte[] EncryptPassword (byte[] password)
		{
			using (SymmetricAlgorithm sa = GetAlgorithm ()) {
				return MachineKeySectionUtils.Encrypt (sa, password);
			}
		}
	}
}
#endif


