namespace Decchi.PublishingModule
{
	internal interface IDecchiPublisher
	{
		bool Login();

		bool Publish(string text);
	}
}
